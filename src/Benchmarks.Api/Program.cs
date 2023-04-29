using System.Threading.RateLimiting;
using Benchmarks.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Benchmarks.Api
{
    internal sealed class Program
    {
        private static readonly List<string> RatelimitTypes = new List<string> { "requests-per-second", "requests-per-sliding-second", "concurrent-requests" };
        private static readonly List<int> RatelimitRequestNumbers = new List<int> { 1, 10, 20, 50, 100 };

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                foreach (var value in RatelimitRequestNumbers)
                {
                    rateLimiterOptions.AddFixedWindowLimiter($"{value}/{RatelimitTypes[0]}", options =>
                    {
                        options.PermitLimit = value;
                        options.Window = TimeSpan.FromSeconds(1);
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });

                    rateLimiterOptions.AddSlidingWindowLimiter($"{value}/{RatelimitTypes[1]}", options =>
                    {
                        options.PermitLimit = value;
                        options.Window = TimeSpan.FromSeconds(1);
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });

                    rateLimiterOptions.AddConcurrencyLimiter($"{value}/{RatelimitTypes[2]}", options =>
                    {
                        options.PermitLimit = value;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });
                }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseRateLimiter();

            foreach (var type in RatelimitTypes)
            {
                foreach (var value in RatelimitRequestNumbers)
                {
                    var policyName = $"{value}/{type}";
                    app.MapGet($"/throttle/{policyName}", () => Results.NoContent() as IActionResult)
                        .RequireRateLimiting(policyName)
                        .WithName($"Throttle {value} {type.Replace('-', ' ')}")
                        .WithOpenApi();
                }
            }

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.MapGet("/ping", async () =>
            {
                await Task.Delay(100);
                return Results.Json(new { message = "pong" });
            })
               .WithName("Ping")
               .WithDescription("Returns 200 OK and pong as the response body");

            app.Run();
        }
    }
}