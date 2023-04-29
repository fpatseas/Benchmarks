namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomWhenAllAsync)]
        public async Task<long[]> UnlimitedCustomWhenAllVersion() => await CustomWhenAllVersion(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomWhenAllAsync)]
        public async Task<long[]> LimitedCustomWhenAllVersion_10() => await CustomWhenAllVersion(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomWhenAllAsync)]
        public async Task<long[]> LimitedCustomWhenAllVersion_20() => await CustomWhenAllVersion(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomWhenAllAsync)]
        public async Task<long[]> LimitedCustomWhenAllVersion_50() => await CustomWhenAllVersion(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.CustomWhenAllAsync)]
        public async Task<long[]> LimitedCustomWhenAllVersion_100() => await CustomWhenAllVersion(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public async Task<long[]> CustomWhenAllVersion(int maxDegreeOfParallelism = -1)
        {
            var batchSize = maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : short.MaxValue;
            var semaphore = new SemaphoreSlim(batchSize);

            var batches = Enumerable.Range(0, TaskCount).ToList().Batch(batchSize);

            var results = new List<long>();

            foreach (var batch in batches)
            {
                var tasks = new List<Task<long>>();
                foreach (var request in batch)
                {
                    // Wait until a semaphore slot is available
                    await semaphore.WaitAsync();

                    // Start the request task and add it to the list
                    var task = GetTestRequest(_httpClient);
                    tasks.Add(task);
                }

                // Wait for all tasks in the batch to complete
                var completed = await Task.WhenAll(tasks);

                // Release the semaphore slots for the completed tasks
                foreach (var _ in batch)
                {
                    semaphore.Release();
                }

                // Add the completed results to the overall results list
                results.AddRange(completed);
            }

            PlotBenchmarkResults(results.ToArray(), $"CustomWhenAllVersion - {maxDegreeOfParallelism}");

            return results.ToArray();
        }
    }
}
