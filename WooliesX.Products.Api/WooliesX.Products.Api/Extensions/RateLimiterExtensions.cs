using System.Threading.RateLimiting;

namespace WooliesX.Products.Api.Extensions;

public static class RateLimiterExtensions
{
    public static WebApplicationBuilder AddIpRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
            options.OnRejected = (context, token) =>
            {
                context.HttpContext.Response.Headers["Retry-After"] = "60";
                return ValueTask.CompletedTask;
            };
        });
        return builder;
    }
}
