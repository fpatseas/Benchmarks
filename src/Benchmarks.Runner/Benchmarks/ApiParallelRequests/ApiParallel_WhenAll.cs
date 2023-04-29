namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeWhenAllAsync)]
        public async Task<long[]> UnlimitedWhenAllVersion() => await WhenAllVersion(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeWhenAllAsync)]
        public async Task<long[]> LimitedWhenAllVersion_10() => await WhenAllVersion(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeWhenAllAsync)]
        public async Task<long[]> LimitedWhenAllVersion_20() => await WhenAllVersion(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeWhenAllAsync)]
        public async Task<long[]> LimitedWhenAllVersion_50() => await WhenAllVersion(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeWhenAllAsync)]
        public async Task<long[]> LimitedWhenAllVersion_100() => await WhenAllVersion(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public async Task<long[]> WhenAllVersion(int maxDegreeOfParallelism)
        {
            // Limit the concurrent number of threads
            var throttler = new SemaphoreSlim(maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : int.MaxValue);

            // Create and run a list of tasks with limited concurrency
            var tasks = new List<Task<long>>(TaskCount);

            for (int i = 0; i < TaskCount; i++)
            {
                await throttler.WaitAsync();

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await GetTestRequest(_httpClient);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            // Await the completion of all tasks
            var results = await Task.WhenAll(tasks);

            PlotBenchmarkResults(results, $"WhenAllVersion - {maxDegreeOfParallelism}");

            return results;
        }
    }
}
