namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.Parallel)]
        public ConcurrentBag<long> UnlimitedParallelVersion() => ParallelVersion(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.Parallel)]
        public ConcurrentBag<long> LimitedParallelVersion_10() => ParallelVersion(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.Parallel)]
        public ConcurrentBag<long> LimitedParallelVersion_20() => ParallelVersion(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.Parallel)]
        public ConcurrentBag<long> LimitedParallelVersion_50() => ParallelVersion(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.Parallel)]
        public ConcurrentBag<long> LimitedParallelVersion_100() => ParallelVersion(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public ConcurrentBag<long> ParallelVersion(int maxDegreeOfParallelism)
        {
            var results = new ConcurrentBag<long>();

            var tasks = Enumerable.Range(0, TaskCount)
               .Select(_ => new Func<long>(() => GetTestRequest(_httpClient).GetAwaiter().GetResult()))
               .ToList();

            Parallel.For(0, TaskCount, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, i => results.Add(tasks[i]()));

            PlotBenchmarkResults(results.ToArray(), $"ParallelVersion - {maxDegreeOfParallelism}");

            return results;
        }
    }
}
