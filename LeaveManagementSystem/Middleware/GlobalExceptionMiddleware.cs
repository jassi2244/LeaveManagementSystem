using System.Net;
using System.Text.Json;

namespace LeaveManagementSystem.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var userId = context.User?.Identity?.IsAuthenticated == true
                ? context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                : "anonymous";

            _logger.LogError(ex, "Unhandled exception. UserId: {UserId}, RequestId: {RequestId}, Path: {Path}",
                userId, context.TraceIdentifier, context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var isAjax = string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            var acceptsJson = context.Request.Headers.Accept.Any(x => x?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
            var isApi = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

            if (isAjax || acceptsJson || isApi)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "An unexpected error occurred." }));
                return;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("""
                                            <!DOCTYPE html>
                                            <html>
                                            <head>
                                                <title>Server Error</title>
                                            </head>
                                            <body>
                                                <h2>Something went wrong.</h2>
                                                <p>Please try again later.</p>
                                            </body>
                                            </html>
                                            """);
        }
    }
}
