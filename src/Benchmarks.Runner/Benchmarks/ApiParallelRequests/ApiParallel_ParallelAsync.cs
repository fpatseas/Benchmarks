namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeParallelAsync)]
        public async Task<List<long>> UnlimitedNativeParallelAsyncVersion() => await NativeParallelAsyncVersion(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeParallelAsync)]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_10() => await NativeParallelAsyncVersion(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeParallelAsync)]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_20() => await NativeParallelAsyncVersion(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeParallelAsync)]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_50() => await NativeParallelAsyncVersion(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.NativeParallelAsync)]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_100() => await NativeParallelAsyncVersion(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public async Task<List<long>> NativeParallelAsyncVersion(int maxDegreeOfParallelism)
        {
            var results = new List<long>(TaskCount);

            var tasks = CreateTasks(_httpClient, TaskCount);

            await Parallel.ForEachAsync(tasks, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, async (request, _) =>
            {
                results.Add(await request());
            });

            PlotBenchmarkResults(results.ToArray(), $"NativeParallelAsyncVersion - {maxDegreeOfParallelism}");

            return results;
        }
    }
}
