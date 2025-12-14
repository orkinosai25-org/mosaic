using System.Diagnostics;
using System.Text;

namespace OrkinosaiCMS.Web.Middleware;

/// <summary>
/// Middleware to log ALL incoming requests at the earliest possible point in the pipeline
/// This catches requests that fail before reaching normal middleware (model binding, antiforgery, etc.)
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        var startTime = Stopwatch.GetTimestamp();
        
        // Log the incoming request BEFORE any processing
        _logger.LogInformation(
            "=> Incoming Request [{TraceId}] - {Method} {Scheme}://{Host}{Path}{QueryString} - ContentType: {ContentType}, ContentLength: {ContentLength}, UserAgent: {UserAgent}, RemoteIP: {RemoteIP}",
            requestId,
            context.Request.Method,
            context.Request.Scheme,
            context.Request.Host,
            context.Request.Path,
            context.Request.QueryString,
            context.Request.ContentType,
            context.Request.ContentLength,
            context.Request.Headers["User-Agent"].ToString(),
            context.Connection.RemoteIpAddress);

        // Log form data availability for POST requests (but not the actual values for security)
        if (context.Request.Method == "POST")
        {
            _logger.LogInformation(
                "=> POST Request [{TraceId}] - HasFormContentType: {HasFormContentType}, IsHttps: {IsHttps}, Cookies: {CookieCount}",
                requestId,
                context.Request.HasFormContentType,
                context.Request.IsHttps,
                context.Request.Cookies.Count);
            
            // Check for antiforgery-related cookies (ASP.NET Core uses cookies with .AspNetCore.Antiforgery prefix)
            var hasAntiforgeryCookie = context.Request.Cookies.Keys.Any(k => 
                k.Contains("Antiforgery", StringComparison.OrdinalIgnoreCase) || 
                k.Contains(".AspNetCore.Antiforgery", StringComparison.OrdinalIgnoreCase));
            
            _logger.LogInformation(
                "=> POST Security [{TraceId}] - HasAntiforgeryCookie: {HasCookie}, CookieNames: [{CookieNames}]",
                requestId,
                hasAntiforgeryCookie,
                string.Join(", ", context.Request.Cookies.Keys.Take(5))); // Log first 5 cookie names for debugging
            
            // Special logging for Blazor Server endpoints
            if (context.Request.Path.StartsWithSegments("/admin") || 
                context.Request.Path.StartsWithSegments("/_blazor"))
            {
                // Check for antiforgery-related headers (optimize by checking once)
                var hasAntiforgeryHeader = context.Request.Headers.ContainsKey("RequestVerificationToken") || 
                                          context.Request.Headers.ContainsKey("X-CSRF-TOKEN");
                
                _logger.LogInformation(
                    "=> Blazor POST [{TraceId}] - Path: {Path}, ContentType: {ContentType}, HasAntiforgeryHeader: {HasHeader}",
                    requestId,
                    context.Request.Path,
                    context.Request.ContentType,
                    hasAntiforgeryHeader);
            }
        }

        Exception? capturedException = null;
        
        try
        {
            // Call the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw; // Re-throw to let error handling middleware process it
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            var statusCode = context.Response.StatusCode;
            
            // Log the response
            var logLevel = statusCode >= 500 ? LogLevel.Error 
                         : statusCode >= 400 ? LogLevel.Warning 
                         : LogLevel.Information;
            
            _logger.Log(logLevel,
                "<= Response [{TraceId}] - {StatusCode} {Method} {Path} - Elapsed: {Elapsed:0.0000}ms{Exception}",
                requestId,
                statusCode,
                context.Request.Method,
                context.Request.Path,
                elapsed.TotalMilliseconds,
                capturedException != null ? $" - Exception: {capturedException.GetType().Name}" : "");
            
            // For error responses, log additional context
            if (statusCode >= 400)
            {
                _logger.Log(logLevel,
                    "<= Error Details [{TraceId}] - StatusCode: {StatusCode}, Path: {Path}, User: {User}, Authenticated: {IsAuthenticated}",
                    requestId,
                    statusCode,
                    context.Request.Path,
                    context.User?.Identity?.Name ?? "Anonymous",
                    context.User?.Identity?.IsAuthenticated ?? false);
            }
        }
    }
}

/// <summary>
/// Extension method to register the request logging middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
