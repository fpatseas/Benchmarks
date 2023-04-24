using System.Collections.Concurrent;
using System.Net;
using BenchmarkDotNet.Attributes;
using Polly;

namespace Benchmarks.Runner.Benchmarks
{
    public class ApiParallelBenchmarks
    {
        private static readonly HttpClient _httpClient = new();
        private const int Concurrency = 100;
        private const string BenchmarksApi_BaseUrl = "https://localhost:7053";

        [Params(100)]
        public int TaskCount { get; set; }

        #region "NativeWhenAllAsync"

        [Benchmark]
        public async Task<long[]> UnlimitedWhenAllVersion() => await WhenAllVersion(-1);

        [Benchmark]
        public async Task<long[]> LimitedWhenAllVersion_10() => await WhenAllVersion(10);

        [Benchmark]
        public async Task<long[]> LimitedWhenAllVersion_20() => await WhenAllVersion(20);

        [Benchmark]
        public async Task<long[]> LimitedWhenAllVersion_50() => await WhenAllVersion(50);

        [Benchmark]
        public async Task<long[]> LimitedWhenAllVersion_100() => await WhenAllVersion(100);

        public async Task<long[]> WhenAllVersion(int maxDegreeOfParallelism)
        {
            // Limit the concurrent number of threads
            var throttler = new SemaphoreSlim(maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : int.MaxValue);

            // Create a list of tasks to run
            var taskFactories = Enumerable.Range(0, TaskCount).Select(i => new Func<Task<long>>(async () =>
            {
                await throttler.WaitAsync();
                try
                {
                    return await GetTestRequest();
                }
                finally
                {
                    throttler.Release();
                }
            }));

            // Now we actually run the tasks
            var results = await Task.WhenAll(taskFactories.Select(factory => factory()));

            PlotBenchmarkResults(results, $"WhenAllVersion - {maxDegreeOfParallelism}");

            return results;
        }

        #endregion

        #region "CustomWhenAllAsync"

        [Benchmark]
        public async Task<long[]> UnlimitedCustomWhenAllVersion() => await CustomWhenAllVersion(-1);

        [Benchmark]
        public async Task<long[]> LimitedCustomWhenAllVersion_10() => await CustomWhenAllVersion(10);

        [Benchmark]
        public async Task<long[]> LimitedCustomWhenAllVersion_20() => await CustomWhenAllVersion(20);

        [Benchmark]
        public async Task<long[]> LimitedCustomWhenAllVersion_50() => await CustomWhenAllVersion(50);

        [Benchmark]
        public async Task<long[]> LimitedCustomWhenAllVersion_100() => await CustomWhenAllVersion(100);

        public async Task<long[]> CustomWhenAllVersion(int maxDegreeOfParallelism = -1)
        {
            var tasks = Enumerable.Range(0, TaskCount)
                .Select(_ => GetTestRequest())
                .ToList();

            var throttler = new SemaphoreSlim(maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : int.MaxValue);

            async Task<long> ExecuteAndRelease(Func<Task<long>> taskFunc)
            {
                long result = await taskFunc();
                throttler.Release();
                return result;
            }

            var results = new List<Task<long>>();
            foreach (var task in tasks)
            {
                await throttler.WaitAsync();
                Func<Task<long>> taskFunc = () => task;
                results.Add(ExecuteAndRelease(taskFunc));
            }

            var output = await Task.WhenAll(results);

            PlotBenchmarkResults(output, $"CustomWhenAllVersion - {maxDegreeOfParallelism}");

            return output;
        }

        #endregion

        #region "PollyBulkhead"

        [Benchmark]
        public async Task<long[]> UnlimitedPollyBulkheadVersion() => await PollyBulkheadVersion(-1);

        [Benchmark]
        public async Task<long[]> LimitedPollyBulkheadVersion_10() => await PollyBulkheadVersion(10);

        [Benchmark]
        public async Task<long[]> LimitedPollyBulkheadVersion_20() => await PollyBulkheadVersion(20);

        [Benchmark]
        public async Task<long[]> LimitedPollyBulkheadVersion_50() => await PollyBulkheadVersion(50);

        [Benchmark]
        public async Task<long[]> LimitedPollyBulkheadVersion_100() => await PollyBulkheadVersion(100);

        public async Task<long[]> PollyBulkheadVersion(int maxDegreeOfParallelism)
        {
            int taskCount = TaskCount;
            var bulkheadPolicy = Policy.Bulkhead(maxDegreeOfParallelism);

            var taskFactories = Enumerable.Range(0, taskCount).Select(i => new Func<Task<long>>(async () =>
            {
                return await GetTestRequest();
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

        #endregion

        #region "NativeParallelAsync"

        [Benchmark]
        public async Task<List<long>> UnlimitedNativeParallelAsyncVersion() => await NativeParallelAsyncVersion(-1);

        [Benchmark]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_10() => await NativeParallelAsyncVersion(10);

        [Benchmark]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_20() => await NativeParallelAsyncVersion(20);

        [Benchmark]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_50() => await NativeParallelAsyncVersion(50);

        [Benchmark]
        public async Task<List<long>> LimitedNativeParallelAsyncVersion_100() => await NativeParallelAsyncVersion(100);

        public async Task<List<long>> NativeParallelAsyncVersion(int maxDegreeOfParallelism)
        {
            var results = new List<long>(TaskCount);

            var tasks = CreateTasks(TaskCount);

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

        #endregion

        #region "CustomParallelAsync"

        [Benchmark]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelVersionAsync_10() => await CustomParallelAsyncVersion(10);

        [Benchmark]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelVersionAsync_20() => await CustomParallelAsyncVersion(20);

        [Benchmark]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelVersionAsync_50() => await CustomParallelAsyncVersion(50);

        [Benchmark]
        public async Task<ConcurrentBag<long>> LimitedCustomParallelAsyncVersion_100() => await CustomParallelAsyncVersion(100);

        public async Task<ConcurrentBag<long>> CustomParallelAsyncVersion(int maxDegreeOfParallelism)
        {
            var results = new ConcurrentBag<long>();

            var tasks = CreateTasks(TaskCount);

            await CustomParallelForEachAsync(tasks, maxDegreeOfParallelism, async func =>
            {
                results.Add(await func());
            });

            PlotBenchmarkResults(results.ToArray(), $"CustomParallelAsyncVersion - {maxDegreeOfParallelism}");

            return results;
        }

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

        #endregion

        #region "Parallel"

        [Benchmark]
        public ConcurrentBag<long> UnlimitedParallelVersion() => ParallelVersion(-1);

        [Benchmark]
        public ConcurrentBag<long> LimitedParallelVersion_10() => ParallelVersion(10);

        [Benchmark]
        public ConcurrentBag<long> LimitedParallelVersion_20() => ParallelVersion(20);

        [Benchmark]
        public ConcurrentBag<long> LimitedParallelVersion_50() => ParallelVersion(50);

        [Benchmark]
        public ConcurrentBag<long> LimitedParallelVersion_100() => ParallelVersion(100);

        public ConcurrentBag<long> ParallelVersion(int maxDegreeOfParallelism)
        {
            var results = new ConcurrentBag<long>();

            var tasks = Enumerable.Range(0, TaskCount)
               .Select(_ => new Func<long>(() => GetTestRequest().GetAwaiter().GetResult()))
               .ToList();

            Parallel.For(0, TaskCount, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, i => results.Add(tasks[i]()));

            PlotBenchmarkResults(results.ToArray(), $"ParallelVersion - {maxDegreeOfParallelism}");

            return results;
        }

        #endregion

        #region "ForEach"

        [Benchmark]
        public async Task<ConcurrentBag<long>> ForEachVersion()
        {
            var tasks = CreateTasks(TaskCount);

            var results = new ConcurrentBag<long>();

            foreach (var task in tasks)
            {
                results.Add(await task());
            }

            PlotBenchmarkResults(results.ToArray(), "ForEachVersion");
            return results;
        }

        #endregion

        #region "Helpers"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<Func<Task<long>>> CreateTasks(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => new Func<Task<long>>(() => GetTestRequest()))
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static async Task<long> GetTestRequest()
        {
            var endpoint = $"{BenchmarksApi_BaseUrl}/ping";
            HttpResponseMessage response;

            do
            {
                response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return DateTime.Now.Ticks;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // TODO: Task.Wait
                }
            } while (response.StatusCode == HttpStatusCode.TooManyRequests);

            return DateTime.Now.Ticks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ratelimit"></param>
        /// <returns></returns>
        private static async Task<int> GetTrottledRequest(int ratelimit)
        {
            var endpoint = $"{BenchmarksApi_BaseUrl}/throttle/{ratelimit}/concurrent-requests";
            HttpResponseMessage response;

            do
            {
                response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return 1;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // TODO: Task.Wait
                }
            } while (response.StatusCode == HttpStatusCode.TooManyRequests);

            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timings"></param>
        /// <param name="label"></param>
        private static void PlotBenchmarkResults(long[] timings, string label)
        {
            var plt = new ScottPlot.Plot(800, 600);

            var minValue = timings.Min();

            double[] doubleArray = new double[timings.Length];

            for (int i = 0; i < timings.Length; i++)
            {
                doubleArray[i] = (double)(timings[i] - minValue);
            }

            plt.AddSignal(doubleArray.OrderBy(d => d).ToArray(), label: label);
            plt.Title("API Call Benchmark");
            plt.XLabel("Request Index");
            plt.YLabel("Elapsed Time (ms)");
            plt.Legend();

            string filename = $"benchmark-{label}.png";
            plt.SaveFig(filename); //Look at the folder of the executable

            Console.WriteLine($"{label} benchmark plot saved as {filename}");
        }

        #endregion
    }
}
