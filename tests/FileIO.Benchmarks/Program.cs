using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace FileIO.Benchmarks
{
    class Program
    {
        static async Task Main(string[] args)
        {
          var summary = BenchmarkRunner.Run<FileIOTest>();

         //var test = new FileIOTest();
         //await test.PipeLines();
        // await test.AsyncStream();
        // test.CsvHelper();
        }
    }
}