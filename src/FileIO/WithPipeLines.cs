﻿#nullable enable
using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace FileIO
{
    public class WithPipeLines
    {
        /// <summary>
        /// Process the file using System.IO.Pipelines
        /// </summary>
        /// <param name="filePath">The file path </param>
        /// <param name="employeeRecords">The Employee Array in which file data will be processed.</param>
        /// <returns>PipeReader Sequence Position</returns>
        public async Task<int> ProcessFileAsync(string filePath, Employee[] employeeRecords)
        {
            var position = 0;
            if (!File.Exists(filePath)) return position;
            await using var fileStream = File.OpenRead(filePath);
            var pipeReader = PipeReader.Create(fileStream);
            return await ReadFromPipe(pipeReader, employeeRecords, position);
        }

        public async Task<int> ProcessWithFullPipeAsync(string filePath, Employee[] employeeRecords)
        {
            var position = 0;
            if (!File.Exists(filePath)) return position;
            await using var fileStream = File.OpenRead(filePath);
            Pipe p = new();
            var fillPipe = FillPipe(fileStream, p.Writer);
            var readPipe = ReadFromPipe(p.Reader, employeeRecords, position);
            await Task.WhenAll(fillPipe, readPipe);

            return await readPipe;
        }

        private static async Task<int> ReadFromPipe(PipeReader pipeReader, Employee[] employeeRecords, int position)
        {
            int pos = position;
            while (true)
            {
                var fileData = await pipeReader.ReadAsync();

                // convert to Buffer
                var fileDataBuffer = fileData.Buffer;

                var sequencePosition = ParseLines(employeeRecords, fileDataBuffer, ref pos);

                pipeReader.AdvanceTo(sequencePosition, fileDataBuffer.End);

                if (fileData.IsCompleted)
                {
                    break;
                }
            }

            await pipeReader.CompleteAsync(); // marking pipe reader as Completed
            return pos;
        }

        private async Task FillPipe(FileStream fileStream, PipeWriter writer)
        {
            await fileStream.CopyToAsync(writer.AsStream());
            await writer.CompleteAsync();
        }

        private static SequencePosition ParseLines(Employee[] employeeRecords, in ReadOnlySequence<byte> buffer, ref int position)
        {
            var reader = new SequenceReader<byte>(buffer);

            // Read the whole line till the new line is found
            while (reader.TryReadTo(out ReadOnlySpan<byte> line, (byte)'\n', true))
            {
                var employee = LineParser.ParseLine(line); // we have a line to parse

                if (employee is { }) // if the returned value is valid Employee object
                    employeeRecords[position++] = employee.Value;
            }

            return reader.Position; // returning the Last position of the reader
        }

        private static class LineParser
        {
            private const byte Coma = (byte)',';
            private static readonly byte[] ColumnHeaders = Encoding.UTF8.GetBytes("Name,Email,DateOfJoining,Salary,Age");

            public static Employee? ParseLine(ReadOnlySpan<byte> line)
            {
                // REVIEW: There are better ways to do this
                if (line.IndexOf(ColumnHeaders) >= 0) // Ignore the Header row
                {
                    return null;
                }

                // Trim \r (if it exists)
                line = line.TrimEnd((byte)'\r');

                var fieldCount = 1;

                var record = new Employee();

                while (fieldCount <= 5) // we have five fields in csv file
                {
                    var comaAt = line.IndexOf(Coma);
                    if (comaAt < 0) // No more comas are found we have reached the last field.
                    {
                        comaAt = line.Length;
                    }

                    switch (fieldCount)
                    {
                        case 1:
                            {
                                var value = Encoding.UTF8.GetString(line[..comaAt]);
                                record.Name = value;
                                break;
                            }
                        case 2:
                            {
                                var value = Encoding.UTF8.GetString(line[..comaAt]);
                                record.Email = value;
                                break;
                            }
                        case 3:
                            {
                                var buffer = line[..comaAt];
                                if (DateTime.TryParse(Encoding.UTF8.GetString(line[..comaAt]), out var doj))

                                {
                                    record.DateOfJoining = doj;
                                }
                                // Can't use Utf8 parser to extract datetime field because csv format doesn't have time
                                //https://docs.microsoft.com/en-us/dotnet/api/system.buffers.text.utf8parser.tryparse?view=net-5.0#System_Buffers_Text_Utf8Parser_TryParse_System_ReadOnlySpan_System_Byte__System_DateTime__System_Int32__System_Char_

                                // if (Utf8Parser.TryParse(buffer, out DateTime value, out var bytesConsumed))
                                // {
                                //     record.DateOfJoining = value;
                                // }
                                break;
                            }

                        case 4:
                            {
                                var buffer = line[..comaAt];
                                if (Utf8Parser.TryParse(buffer, out double value, out var bytesConsumed))
                                {
                                    record.Salary = value;
                                }
                                break;
                            }

                        case 5:
                            {
                                var buffer = line[..comaAt];
                                if (Utf8Parser.TryParse(buffer, out short value, out var bytesConsumed))
                                {
                                    record.Age = value;
                                }
                                return record;
                            }
                    }

                    line = line[(comaAt + 1)..]; // slice past field

                    fieldCount++;
                }

                return record;
            }
        }
    }
}