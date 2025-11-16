using System.Diagnostics;

namespace WooliesX.Products.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next = next;
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Ensure an Activity exists so TraceId is available
        Activity? started = null;
        if (Activity.Current == null)
        {
            started = new Activity("Correlation");
            started.Start();
        }

        try
        {
            var incoming = context.Request.Headers.TryGetValue(HeaderName, out var values)
                ? values.ToString()
                : null;

            var correlationId = !string.IsNullOrWhiteSpace(incoming)
                ? incoming!
                : (Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier);

            if (string.IsNullOrWhiteSpace(incoming))
            {
                context.Request.Headers[HeaderName] = correlationId;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            using (_logger.BeginScope(new Dictionary<string, object> { [HeaderName] = correlationId }))
            {
                await _next(context);
            }
        }
        finally
        {
            started?.Stop();
        }
    }
}
