#nullable enable
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
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
            const int BufferSize = 0x10000;
            var position = 0;
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize);
            var pipeReader = PipeReader.Create(fileStream, new StreamPipeReaderOptions(bufferSize: BufferSize));
            while (true)
            {
                var fileData = await pipeReader.ReadAsync();

                // convert to Buffer
                var fileDataBuffer = fileData.Buffer;

                var sequencePosition = ParseLines(employeeRecords, fileDataBuffer, ref position);

                pipeReader.AdvanceTo(sequencePosition, fileDataBuffer.End);

                if (fileData.IsCompleted)
                {
                    break;
                }
            }

            await pipeReader.CompleteAsync(); // marking pipereader as Completed
            return position;
        }

        private static SequencePosition ParseLines(Employee[] employeeRecords, in ReadOnlySequence<byte> buffer, ref int position)
        {
            var reader = new SequenceReader<byte>(buffer);
            ReadOnlySpan<byte> line;

            // skip the header row
            reader.TryReadTo(out line, (byte)'\n', true);
            // Read the whole line till the new line is found
            while (reader.TryReadTo(out line, (byte)'\n', true))
            {
                var employee = LineParser.ParseLine(line); // we have a line to parse

                employeeRecords[position++] = employee;
            }

            return reader.Position; // returning the Last position of the reader
        }

        private static class LineParser
        {
            private const byte Coma = (byte)',';

            public static Employee ParseLine(ReadOnlySpan<byte> line)
            {
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
                                // stand on our heads to avoid allocating a temp string to parse the date
                                var buffer = line[..comaAt];
                                Span<char> chars = stackalloc char[buffer.Length];
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    chars[i] = (char)buffer[i];
                                }
                                if (DateTime.TryParse(chars, out var doj))
                                {
                                    record.DateOfJoining = doj;
                                }
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