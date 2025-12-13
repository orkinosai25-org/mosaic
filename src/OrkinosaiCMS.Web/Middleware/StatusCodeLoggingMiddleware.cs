using System.Diagnostics;

namespace OrkinosaiCMS.Web.Middleware;

/// <summary>
/// Middleware to log HTTP status codes, especially 4xx and 5xx errors
/// This catches errors that don't throw exceptions (like Bad Request, validation errors, antiforgery failures)
/// </summary>
public class StatusCodeLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StatusCodeLoggingMiddleware> _logger;

    public StatusCodeLoggingMiddleware(
        RequestDelegate next,
        ILogger<StatusCodeLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = Stopwatch.GetTimestamp();
        
        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            var statusCode = context.Response.StatusCode;
            
            // Log 4xx and 5xx status codes with detailed information
            if (statusCode >= 400)
            {
                var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                
                _logger.Log(logLevel,
                    "HTTP {StatusCode} - {Method} {Path}{QueryString} - {UserAgent} - Elapsed: {Elapsed:0.0000}ms - TraceId: {TraceId}",
                    statusCode,
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Request.Headers["User-Agent"].ToString(),
                    elapsed.TotalMilliseconds,
                    context.TraceIdentifier);
                
                // Log additional details for specific error types
                if (statusCode == 400)
                {
                    _logger.LogWarning(
                        "Bad Request Details - ContentType: {ContentType}, ContentLength: {ContentLength}, Referer: {Referer}",
                        context.Request.ContentType,
                        context.Request.ContentLength,
                        context.Request.Headers["Referer"].ToString());
                    
                    // Check for antiforgery token issues
                    if (context.Request.HasFormContentType)
                    {
                        _logger.LogWarning("Request has form content - may indicate antiforgery validation failure or invalid model binding");
                    }
                }
                else if (statusCode == 401)
                {
                    _logger.LogWarning(
                        "Unauthorized access attempt - Path: {Path}, User: {User}",
                        context.Request.Path,
                        context.User?.Identity?.Name ?? "Anonymous");
                }
                else if (statusCode == 404)
                {
                    _logger.LogInformation(
                        "Resource not found - Path: {Path}",
                        context.Request.Path);
                }
                else if (statusCode == 500)
                {
                    _logger.LogError(
                        "Internal Server Error - Check exception logs for details");
                }
                else if (statusCode == 503)
                {
                    _logger.LogError(
                        "Service Unavailable - Application may be overloaded or dependency is down");
                }
            }
        }
    }
}

/// <summary>
/// Extension method to register the status code logging middleware
/// </summary>
public static class StatusCodeLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseStatusCodeLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<StatusCodeLoggingMiddleware>();
    }
}
