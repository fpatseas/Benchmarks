namespace Benchmarks.Runner.Common
{
    public static class Extensions
    {
        public static IEnumerable<List<T>> Batch<T>(this List<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
            {
                yield return source.GetRange(i, Math.Min(batchSize, source.Count - i));
            }
        }
    }
}
