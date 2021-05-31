using System.Buffers;
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
        public async Task AsyncStream()
        { 
            var asyncStream = new WithAsyncStreams();
           var employees = await asyncStream.ProcessStreamAsync(_filePath);
        }

        [Benchmark]
        public void CsvHelper()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program))?.Location);
            _filePath = Path.Combine(directoryPath ?? string.Empty, "Employees.csv");
            var csvHelper = new WithCsvHelperLib();
            var employeesList = csvHelper.ProcessFileAsync(_filePath);

        }
    }
}