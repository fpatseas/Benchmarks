using BenchmarkDotNet.Running;
using Benchmarks.Runner.Benchmarks;

namespace Benchmarks.Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ApiParallelBenchmarks>();

            Console.ReadLine();
        }
    }
}