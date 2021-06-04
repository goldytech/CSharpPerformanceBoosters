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
        private string _filePath = "Employees.csv";

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
        public async Task<IList<Employee>> AsyncStream()
        {
            var asyncStream = new WithAsyncStreams();
            var employees = await asyncStream.ProcessStreamAsync(_filePath);
            return employees;
        }

        [Benchmark]
        public void CsvHelper()
        {
            var csvHelper = new WithCsvHelperLib();
            var employeesList = csvHelper.ProcessFileAsync(_filePath);

        }

        [Benchmark]
        public void Sylvan()
        {
            var sylv = new WithSylvanLib();
            var pool = ArrayPool<Employee>.Shared;
            var employeeRecords = pool.Rent(100000);

            try
            {
                sylv.ProcessFile(_filePath, employeeRecords);
            }
            finally
            {
                pool.Return(employeeRecords, true);
            }
        }

        [Benchmark]
        public async Task SylvanAsync()
        {
            var sylv = new WithSylvanLib();
            var pool = ArrayPool<Employee>.Shared;
            var employeeRecords = pool.Rent(100000);

            try
            {
                await sylv.ProcessFileAsync(_filePath, employeeRecords);
            }
            finally
            {
                pool.Return(employeeRecords, true);
            }
        }
    }
}
