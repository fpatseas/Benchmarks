using System.Net;
using BenchmarkDotNet.Attributes;

namespace Benchmarks.Runner.Benchmarks
{
    [MemoryDiagnoser]
    public class ApiParallelBenchmarks
    {
        private static readonly HttpClient _httpClient = new();
        private const int Concurrency = 10;
        private const int TaskCount = 1000;

        [Benchmark]
        public async Task<List<int>> ForEachVersion()
        {
            var list = new List<int>();
            var tasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<Task<int>>(() => GetTrottledRequest(Concurrency)))
                .ToList();

            foreach (var task in tasks)
            {
                list.Add(await task());
            }

            return list;
        }

        [Benchmark]
        public List<int> ParallelVersion()
        {
            var list = new List<int>();
            var requests = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<int>(() => GetTrottledRequest(Concurrency).GetAwaiter().GetResult()))
                .ToList();

            Parallel.For(0, requests.Count, i => list.Add(requests[i]()));

            return list;
        }

        private static async Task<int> GetTrottledRequest(int ratelimit)
        {
            var endpoint = $"https://localhost:7053/throttle/{ratelimit}/concurrent-requests";
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
    }
}
