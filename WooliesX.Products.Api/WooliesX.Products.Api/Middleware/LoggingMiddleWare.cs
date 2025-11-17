using System.Diagnostics;
using System.Text;

namespace WooliesX.Products.Api.Middleware;

public class LoggingMiddleWare(RequestDelegate next, ILogger<LoggingMiddleWare> logger)
{
    private const int MaxBodyBytes = 64 * 1024;
    private readonly RequestDelegate _next = next;
    private readonly ILogger<LoggingMiddleWare> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Detect Server-Sent Events by Accept header or path pattern
        var isSseRequest = IsSseRequest(context.Request);

        if (isSseRequest)
        {
            // Log SSE request start without buffering
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            _logger.LogInformation(
                "SSE Stream Started: {Method} {PathQuery} | CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path + context.Request.QueryString,
                correlationId);

            var requestSW = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                requestSW.Stop();
                _logger.LogInformation(
                    "SSE Stream Completed: {Method} {PathQuery} in {ElapsedMs} ms | CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path + context.Request.QueryString,
                    requestSW.ElapsedMilliseconds,
                    correlationId);
            }
            return;
        }

        // Normal request/response logging with buffering
        context.Request.EnableBuffering();
        var requestBody = await ReadRequestBodyAsync(context.Request);

        var originalBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            // Check if response became SSE after processing
            var isSseResponse = context.Response.ContentType?.Contains("text/event-stream",
                StringComparison.OrdinalIgnoreCase) ?? false;

            if (isSseResponse)
            {
                // If somehow we missed SSE detection, don't buffer
                responseBuffer.Seek(0, SeekOrigin.Begin);
                await responseBuffer.CopyToAsync(originalBody);
                context.Response.Body = originalBody;

                var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                    ?? context.TraceIdentifier;
                _logger.LogInformation(
                    "SSE Response Detected: {Method} {PathQuery} -> {StatusCode} in {ElapsedMs} ms | CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path + context.Request.QueryString,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    correlationId);
            }
            else
            {
                // Normal response logging
                responseBuffer.Seek(0, SeekOrigin.Begin);
                var responseBody = await ReadBodyFromStreamAsync(responseBuffer, context.Response.ContentType);
                responseBuffer.Seek(0, SeekOrigin.Begin);
                await responseBuffer.CopyToAsync(originalBody);
                context.Response.Body = originalBody;

                var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                    ?? context.TraceIdentifier;

                _logger.LogInformation(
                    "Handled {Method} {PathQuery} -> {StatusCode} in {ElapsedMs} ms | CorrelationId={CorrelationId}\nRequestBody: {RequestBody}\nResponseBody: {ResponseBody}",
                    context.Request.Method,
                    context.Request.Path + context.Request.QueryString,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    correlationId,
                    requestBody,
                    responseBody);
            }
        }
    }

    private static bool IsSseRequest(HttpRequest request)
    {
        // Check Accept header
        if (request.Headers.TryGetValue("Accept", out var accept)
            && accept.Any(a => a?.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return true;
        }

        // Check for common SSE endpoint patterns
        var path = request.Path.Value ?? string.Empty;
        if (path.Contains("/stream", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("/sse", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/events", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.Body == null)
            return string.Empty;

        request.Body.Position = 0;
        var contentType = request.ContentType ?? string.Empty;
        var length = request.ContentLength ?? 0;

        string text;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
        {
            text = await reader.ReadToEndAsync();
        }
        request.Body.Position = 0;

        return SanitizeBody(text, contentType, length);
    }

    private static async Task<string> ReadBodyFromStreamAsync(Stream stream, string? contentType)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        var text = await reader.ReadToEndAsync();
        var memoryLength = stream is MemoryStream ms ? ms.Length : text.Length;
        return SanitizeBody(text, contentType ?? string.Empty, memoryLength);
    }

    private static string SanitizeBody(string text, string? contentType, long? length)
    {
        var ct = contentType ?? string.Empty;
        var isText = ct.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                     || ct.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                     || ct.Contains("application/xml", StringComparison.OrdinalIgnoreCase)
                     || string.IsNullOrWhiteSpace(ct);

        if (!isText)
        {
            return $"[Non-text body omitted: ContentType={ct}, Length={(length?.ToString() ?? "?")}]";
        }

        if (length.HasValue && length.Value > MaxBodyBytes)
        {
            var truncated = Encoding.UTF8.GetBytes(text);
            var slice = truncated.Length > MaxBodyBytes
                ? Encoding.UTF8.GetString(truncated, 0, MaxBodyBytes)
                : text;
            return slice + $"… [truncated to {MaxBodyBytes} bytes]";
        }

        return text;
    }
}