#nullable enable
using System;
using System.Buffers;
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
        /// <param name="filePath"></param>
        /// <param name="employeeRecords"></param>
        /// <returns></returns>
        public async Task<int> ProcessFileAsync(string filePath, Employee[] employeeRecords)
        {
            var position = 0;
            if (!File.Exists(filePath)) return position;
            await using var fileStream = File.OpenRead(filePath);
            var pipeReader = PipeReader.Create(fileStream);
            while (true)
            {
                var fileData = await pipeReader.ReadAsync();

                // convert to Buffer
                var fileDataBuffer = fileData.Buffer;
                
               
                var sequencePosition = ParseLines(employeeRecords, ref fileDataBuffer, ref position);

                pipeReader.AdvanceTo(sequencePosition, fileDataBuffer.End);

                if (fileData.IsCompleted)
                {
                    break;
                }
            }

            await pipeReader.CompleteAsync();
            return position;
        }
        
         private static SequencePosition ParseLines(Employee[] employeeRecords, ref ReadOnlySequence<byte> buffer, ref int position)
        {
            var newLine = Encoding.UTF8.GetBytes(Environment.NewLine).AsSpan();

            var reader = new SequenceReader<byte>(buffer);

            while (!reader.End)
            {
                // Read the whole line till the new line is found
                if (!reader.TryReadToAny(out ReadOnlySpan<byte> line, newLine, true))
                {
                    break; // we don't have a delimiter (newline) in the current data
                }

                var parsedLine = LineParser.ParseLine(line); // we have a line to parse

                if (parsedLine is { }) // if the returned value is valid Employee object
                    employeeRecords[position++] = (Employee) parsedLine;
            }

            return reader.Position; // returning the Last position of the reader
        }

        private static class LineParser
        {
            private const byte Coma = (byte) ',';
            private const string ColumnHeaders = "Name,Email,DateOfJoining,Salary,Age";
            public static Employee? ParseLine(ReadOnlySpan<byte> line)
            {
                if (Encoding.UTF8.GetString(line).Contains(ColumnHeaders))
                {
                    return null;
                }
                var fieldCount = 1;

                var record = new Employee();

                while (fieldCount <= 5) // we have five fields in csv file
                {
                     var comaAt = line.IndexOf(Coma);
                     if (comaAt < 0)
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
                             var value = Encoding.UTF8.GetString(line[..comaAt]);
                             record.DateOfJoining = Convert.ToDateTime(value);
                             break;
                         }
                        
                         case 4:
                         {
                             var value = Encoding.UTF8.GetString(line[..comaAt]);
                             record.Salary = Convert.ToDouble(value);
                             break;
                         }
                        
                         case 5:
                         {
                             var value = Encoding.UTF8.GetString(line[..comaAt]);
                             record.Age = Convert.ToInt16(value);
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