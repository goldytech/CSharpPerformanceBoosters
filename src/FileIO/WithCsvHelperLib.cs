using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace FileIO
{
    public class WithCsvHelperLib
    {

        public  IEnumerable<Employee> ProcessFileAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<Employee>();
            return records.ToList();

        }
    }
}