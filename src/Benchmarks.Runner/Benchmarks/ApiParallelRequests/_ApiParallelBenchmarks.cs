namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [AnyCategoriesFilter(
    //ApiBenchmarks.NativeWhenAllAsync
    //ApiBenchmarks.CustomWhenAllAsync
    ApiBenchmarks.ActorsWhenAllAsync
    //, ApiBenchmarks.PollyBulkhead
    //,ApiBenchmarks.NativeParallelAsync
    //,ApiBenchmarks.CustomParallelAsync
    //,ApiBenchmarks.Parallel
    //,ApiBenchmarks.ForEach
    )]
    internal sealed partial class ApiParallelBenchmarks : BenchmarksBase
    {
        private static readonly HttpClient _httpClient = new();

        [Params(100)]
        public int TaskCount { get; set; }
    }
}
