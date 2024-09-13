``` ini

BenchmarkDotNet=v0.13.1, OS=macOS 14.6.1 (23G93) [Darwin 23.6.0]
Intel Core i9-9980HK CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT
  DefaultJob : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT


```
|     Method |     Mean |    Error |   StdDev |     Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|----------- |---------:|---------:|---------:|----------:|----------:|----------:|----------:|
| LookupTest | 370.4 ms | 11.85 ms | 34.94 ms | 7000.0000 | 4000.0000 | 1000.0000 |     66 MB |
