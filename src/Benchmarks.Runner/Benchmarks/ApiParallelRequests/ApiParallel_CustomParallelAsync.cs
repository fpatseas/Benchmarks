namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomParallelAsync)]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelVersionAsync_10() => await CustomParallelAsyncVersion(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomParallelAsync)]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelVersionAsync_20() => await CustomParallelAsyncVersion(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomParallelAsync)]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelVersionAsync_50() => await CustomParallelAsyncVersion(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomParallelAsync)]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelAsyncVersion_100() => await CustomParallelAsyncVersion(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public async Task<ConcurrentBag<long>> CustomParallelAsyncVersion(int maxDegreeOfParallelism)
        {
            var results = new ConcurrentBag<long>();

            var tasks = CreateTasks(_httpClient, TaskCount);

            await CustomParallelForEachAsync(tasks, maxDegreeOfParallelism, async func =>
            {
                results.Add(await func());
            });

            PlotBenchmarkResults(results.ToArray(), $"CustomParallelAsyncVersion - {maxDegreeOfParallelism}");

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="degreeOfParallelization"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static Task CustomParallelForEachAsync<T>(
            IEnumerable<T> source,
            int degreeOfParallelization,
            Func<T, Task> body)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await body(partition.Current);
                    }
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(degreeOfParallelization)
                    .AsParallel()
                    .Select(AwaitPartition));
        }
    }
}
