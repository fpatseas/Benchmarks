namespace Benchmarks.Runner.Common
{
    public static class Constants
    {
        public static class ApiBenchmarks
        {
            public const string NativeWhenAllAsync = "NativeWhenAllAsync";
            public const string CustomWhenAllAsync = "CustomWhenAllAsync";
            public const string ActorsWhenAllAsync = "ActorsWhenAllAsync";
            public const string PollyBulkhead = "PollyBulkhead";
            public const string NativeParallelAsync = "NativeParallelAsync";
            public const string CustomParallelAsync = "CustomParallelAsync";
            public const string Parallel = "Parallel";
            public const string ForEach = "ForEach";
        }
    }
}
