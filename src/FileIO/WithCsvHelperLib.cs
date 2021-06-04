using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace FileIO
{
    public class WithCsvHelperLib
    {

        public IEnumerable<Employee> ProcessFileAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);

            Employee[] employees = new Employee[100000];
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            int idx = 0;
            csv.Read();
            while (csv.Read())
            {
                var emp = new Employee
                {
                    Name = csv[0],
                    Email = csv[1],
                    DateOfJoining = DateTime.Parse(csv[2]),
                    Salary = double.Parse(csv[3]),
                    Age = int.Parse(csv[4]),
                };
                employees[idx++] = emp;

            }
            return employees;

        }
    }
}