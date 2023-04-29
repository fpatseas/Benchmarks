using Benchmarks.Runner.Benchmarks.ApiParallelRequests;

namespace Benchmarks.Runner
{
    internal sealed class Program
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