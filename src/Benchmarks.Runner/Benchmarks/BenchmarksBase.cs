using System.Net;

namespace Benchmarks.Runner.Benchmarks
{
    public abstract class BenchmarksBase
    {
        public const string BenchmarksApiBaseUrl = "https://localhost:7053";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<Func<Task<long>>> CreateTasks(HttpClient httpClient, int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => new Func<Task<long>>(() => GetTestRequest(httpClient)))
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<long> GetTestRequest(HttpClient httpClient)
        {
            var endpoint = $"{BenchmarksApiBaseUrl}/ping";
            HttpResponseMessage response;

            do
            {
                response = await httpClient.GetAsync(endpoint);

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
        public static async Task<int> GetTrottledRequest(HttpClient httpClient, int ratelimit)
        {
            var endpoint = $"{BenchmarksApiBaseUrl}/throttle/{ratelimit}/concurrent-requests";
            HttpResponseMessage response;

            do
            {
                response = await httpClient.GetAsync(endpoint);

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
        public static void PlotBenchmarkResults(long[] timings, string label)
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

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BenchmarkResults");
            Directory.CreateDirectory(folderPath);
            string filename = Path.Combine(folderPath, $"benchmark-{label}.png");
            plt.SaveFig(filename);

            Console.WriteLine($"{label} benchmark plot saved as {filename}");
        }
    }
}
