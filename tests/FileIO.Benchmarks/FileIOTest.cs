using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace FileIO.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn()]
    public class FileIOTest
    {
        private string _filePath;
        [GlobalSetup]
        public void Setup()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program))?.Location);
            _filePath = Path.Combine(directoryPath ?? string.Empty, "Employees.csv");
        }
        [Benchmark]
        public async Task PipeLines()
        {
           // var directoryPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program))?.Location);
            //_filePath = Path.Combine(directoryPath ?? string.Empty, "Employees.csv");
            var pool = ArrayPool<Employee>.Shared;
            var employeeRecords = pool.Rent(100000);
            var pipeLinesTest = new WithPipeLines();

            try
            {
                await pipeLinesTest.ProcessFileAsync(_filePath, employeeRecords);
            }
            finally
            {
                pool.Return(employeeRecords, clearArray: true);
            }
        }

        [Benchmark]
        public async Task<IList<Employee>> AsyncStream()
        { 
          //  var directoryPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program))?.Location);
           // _filePath = Path.Combine(directoryPath ?? string.Empty, "Employees.csv");
            var asyncStream = new WithAsyncStreams();
           var employees = await asyncStream.ProcessStreamAsync(_filePath);
           return employees;
        }

        [Benchmark]
        public void CsvHelper()
        {
          //  var directoryPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program))?.Location);
            //_filePath = Path.Combine(directoryPath ?? string.Empty, "Employees.csv");
            var csvHelper = new WithCsvHelperLib();
            var employeesList = csvHelper.ProcessFileAsync(_filePath);

        }

        [Benchmark]
        public void Sylvan()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program))?.Location);
            _filePath = Path.Combine(directoryPath ?? string.Empty, "Employees.csv");
            var sylv = new WithSylvanLib();
            var pool = ArrayPool<Employee>.Shared;
            var employeeRecords = pool.Rent(100000);

            try {
                sylv.ProcessFile(_filePath, employeeRecords);
            } finally {
                pool.Return(employeeRecords, true);
            }
        }
    }
}
