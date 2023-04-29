namespace Benchmarks.Api.Middlewares
{
    internal sealed class RequestLoggingMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly RequestDelegate _next;
        private readonly ILogger<Program> _logger;
        private static int _concurrentCallCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<Program> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Increment the concurrent call counter
            Interlocked.Increment(ref _concurrentCallCount);

            _logger.LogInformation($"Handling request for {context.Request.Path}");

            // Log the concurrent call number
            _logger.LogInformation($"Current concurrent calls: {_concurrentCallCount}");

            // Call the next middleware in the pipeline
            await _next(context);

            // Decrement the concurrent call counter
            Interlocked.Decrement(ref _concurrentCallCount);

            _logger.LogInformation($"Finished handling request for {context.Request.Path}");
        }
    }
}
