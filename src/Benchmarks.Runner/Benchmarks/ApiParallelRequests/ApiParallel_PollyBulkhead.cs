using Polly;

namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.PollyBulkhead)]
        public async Task<long[]> UnlimitedPollyBulkheadVersion() => await PollyBulkheadVersion(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.PollyBulkhead)]
        public async Task<long[]> LimitedPollyBulkheadVersion_10() => await PollyBulkheadVersion(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.PollyBulkhead)]
        public async Task<long[]> LimitedPollyBulkheadVersion_20() => await PollyBulkheadVersion(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.PollyBulkhead)]
        public async Task<long[]> LimitedPollyBulkheadVersion_50() => await PollyBulkheadVersion(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.PollyBulkhead)]
        public async Task<long[]> LimitedPollyBulkheadVersion_100() => await PollyBulkheadVersion(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public async Task<long[]> PollyBulkheadVersion(int maxDegreeOfParallelism)
        {
            int taskCount = TaskCount;
            var bulkheadPolicy = Policy.Bulkhead(maxDegreeOfParallelism);

            var taskFactories = Enumerable.Range(0, taskCount).Select(i => new Func<Task<long>>(async () =>
            {
                return await GetTestRequest(_httpClient);
            }));

            async Task<long> ExecuteWithBulkheadPolicy(Func<Task<long>> taskFactory)
            {
                return await bulkheadPolicy.Execute(taskFactory);
            }

            var tasks = taskFactories.Select(ExecuteWithBulkheadPolicy).ToArray();
            long[] results = await Task.WhenAll(tasks);

            PlotBenchmarkResults(results, $"PollyBulkhead - {maxDegreeOfParallelism}");

            return results;
        }
    }
}
