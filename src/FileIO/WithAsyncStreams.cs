using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileIO
{
    public class WithAsyncStreams
    {
        private async IAsyncEnumerable<string> ReadStream(string filePath)
        {
            var rdr = new StreamReader(filePath);
            while (!rdr.EndOfStream)
            {
                var line = await rdr.ReadLineAsync();

                yield return line;
            }
        }


        /// <summary>
        /// Process Data stream
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of Employees</returns>
        public async Task<IList<Employee>> ProcessStreamAsync(string filePath)
        {
            var employeesList = await ReadStream(filePath)
                .Skip(1) //skip header row
                .Select(e => e)
                .ToListAsync();


            return employeesList.Select(emp => emp.Split(","))
                .Select(employeeLine => new Employee
                {
                    Name = employeeLine[0],
                    Email = employeeLine[1],
                    DateOfJoining = Convert.ToDateTime(employeeLine[2], CultureInfo.InvariantCulture.DateTimeFormat),
                    Salary = Convert.ToDouble(employeeLine[3]),
                    Age = Convert.ToInt16(employeeLine[4])
                })
                .ToList();

          
        }
    }
}
