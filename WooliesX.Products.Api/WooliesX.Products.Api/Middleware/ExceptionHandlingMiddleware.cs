using System.Net;
using System.Text.Json;

namespace WooliesX.Products.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var (status, title, detail, errors) = MapException(exception);

        _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new Dictionary<string, object?>
        {
            ["type"] = "about:blank",
            ["title"] = title,
            ["status"] = (int)status,
            ["detail"] = detail,
            ["traceId"] = traceId
        };
        if (errors is not null)
        {
            problem["errors"] = errors;
        }

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode status, string title, string detail, IDictionary<string, string[]>? errors) MapException(Exception ex)
    {
        return ex switch
        {
            ValidationException vex => (HttpStatusCode.BadRequest, "Validation Failed", vex.Message, new Dictionary<string, string[]>(vex.Errors)),
            MissingBodyException mbe => (HttpStatusCode.BadRequest, "Bad Request", mbe.Message, null),
            ArgumentException arge => (HttpStatusCode.BadRequest, "Bad Request", arge.Message, null),
            DuplicateProductException dpex => (HttpStatusCode.Conflict, "Duplicate Product", dpex.Message, null),
            KeyNotFoundException knf => (HttpStatusCode.NotFound, "Not Found", knf.Message, null),
            _ => (HttpStatusCode.InternalServerError, "Server Error", "An unexpected error occurred.", null)
        };
    }
}
