using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Benchmarks.Runner.Benchmarks;

namespace Benchmarks.Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(false)));

            BenchmarkRunner.Run<ApiParallelBenchmarks>(config);

            Console.ReadLine();
        }
    }
}