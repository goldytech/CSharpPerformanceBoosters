# CSharpPerformanceBoosters

###### Highly Performant C# code with benchmark results

#### File I/O

1) System.IO.Pipelines
2) IAsyncEnumerable (Async stream)
3) CSVHelper

``` ini

BenchmarkDotNet=v0.13.0, OS=macOS Big Sur 11.4 (20F71) [Darwin 20.5.0]
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=5.0.203
  [Host]     : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT
  DefaultJob : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT


```
|      Method |     Mean |   Error |  StdDev | Rank |      Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|------------ |---------:|--------:|--------:|-----:|-----------:|----------:|----------:|----------:|
|   PipeLines | 143.1 ms | 2.81 ms | 3.24 ms |    1 |  5500.0000 | 2000.0000 |  750.0000 |     44 MB |
| AsyncStream | 223.0 ms | 4.40 ms | 8.36 ms |    2 |  8000.0000 | 3000.0000 | 1000.0000 |     64 MB |
|   CsvHelper | 228.5 ms | 4.51 ms | 7.29 ms |    3 | 11000.0000 | 5000.0000 | 3000.0000 |     77 MB |


