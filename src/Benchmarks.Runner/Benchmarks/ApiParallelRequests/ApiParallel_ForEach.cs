namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.ForEach)]
        public async Task<ConcurrentBag<long>> ForEachVersion()
        {
            var tasks = CreateTasks(_httpClient, TaskCount);

            var results = new ConcurrentBag<long>();

            foreach (var task in tasks)
            {
                results.Add(await task());
            }

            PlotBenchmarkResults(results.ToArray(), "ForEachVersion");
            return results;
        }
    }
}
