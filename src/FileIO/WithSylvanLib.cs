using System.IO;
using Sylvan.Data.Csv;
using System.Buffers;

namespace FileIO
{
    public class WithSylvanLib
    {
        public void ProcessFile(string filePath, Employee[] employeeRecords)
        {
            using var reader = new StreamReader(filePath);

            char[] buffer = ArrayPool<char>.Shared.Rent(0x10000);

            using var csv = CsvDataReader.Create(reader, new CsvDataReaderOptions { Buffer = buffer });
            int idx = 0;
            while (csv.Read())
            {
                employeeRecords[idx++] = new Employee
                {
                    Name = csv.GetString(0),
                    Email = csv.GetString(1),
                    DateOfJoining = csv.GetDateTime(2),
                    Salary = csv.GetDouble(3),
                    Age = csv.GetInt32(4),
                };
            }
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
