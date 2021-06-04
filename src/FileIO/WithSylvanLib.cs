using System.IO;
using Sylvan.Data.Csv;
using System.Buffers;
using System.Threading.Tasks;
using System.Text;

namespace FileIO
{
    public class WithSylvanLib
    {
        public void ProcessFile(string filePath, Employee[] employeeRecords)
        {
            const int BufferSize = 0x10000;
            using var reader = new StreamReader(filePath, Encoding.UTF8, false, BufferSize);

            char[] buffer = ArrayPool<char>.Shared.Rent(BufferSize);

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

        public async Task ProcessFileAsync(string filePath, Employee[] employeeRecords)
        {
            const int BufferSize = 0x10000;
            using var reader = new StreamReader(filePath, Encoding.UTF8, false, BufferSize);

            char[] buffer = ArrayPool<char>.Shared.Rent(BufferSize);

            await using var csv = await CsvDataReader.CreateAsync(reader, new CsvDataReaderOptions { Buffer = buffer });
            int idx = 0;
            while (await csv.ReadAsync())
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
