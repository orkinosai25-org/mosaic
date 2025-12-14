using System.Net;
using System.Text.Json;

namespace OrkinosaiCMS.Web.Middleware;

/// <summary>
/// Global exception handler middleware that logs all unhandled exceptions with Serilog
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Check if this is an antiforgery validation failure
            var isAntiforgeryError = IsAntiforgeryException(ex);
            if (isAntiforgeryError)
            {
                context.Items["AntiforgeryError"] = true;
                _logger.LogError(ex, 
                    "ANTIFORGERY VALIDATION FAILED - Path: {Path}, Method: {Method}, TraceId: {TraceId}, HasAntiforgeryCookie: {HasCookie}, IsHttps: {IsHttps}",
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier,
                    context.Request.Cookies.Keys.Any(k => k.Contains("Antiforgery", StringComparison.OrdinalIgnoreCase)),
                    context.Request.IsHttps);
            }

            // Check if this is a database connectivity error
            var isDatabaseError = IsDatabaseException(ex);
            if (isDatabaseError)
            {
                context.Items["DatabaseError"] = true;
            }

            // Log the exception with full details
            _logger.LogError(ex, 
                "Unhandled exception occurred. Path: {Path}, Method: {Method}, StatusCode: {StatusCode}, TraceId: {TraceId}, IsDatabaseError: {IsDatabaseError}, IsAntiforgeryError: {IsAntiforgeryError}",
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode,
                context.TraceIdentifier,
                isDatabaseError,
                isAntiforgeryError);

            // Log additional context
            LogExceptionDetails(ex, context);

            // Re-throw to let ASP.NET Core's exception handler handle the response
            throw;
        }
    }

    private bool IsAntiforgeryException(Exception ex)
    {
        // Check for antiforgery validation exceptions
        var exceptionType = ex.GetType().FullName ?? "";
        
        // Check current exception using case-insensitive comparison
        if (exceptionType.Contains("AntiforgeryValidationException", StringComparison.OrdinalIgnoreCase) || 
            exceptionType.Contains("Antiforgery", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("antiforgery", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("invalid token", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("the antiforgery token", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check inner exceptions
        var innerException = ex.InnerException;
        while (innerException != null)
        {
            var innerType = innerException.GetType().FullName ?? "";
            
            if (innerType.Contains("AntiforgeryValidationException", StringComparison.OrdinalIgnoreCase) || 
                innerType.Contains("Antiforgery", StringComparison.OrdinalIgnoreCase) ||
                innerException.Message.Contains("antiforgery", StringComparison.OrdinalIgnoreCase) ||
                innerException.Message.Contains("invalid token", StringComparison.OrdinalIgnoreCase) ||
                innerException.Message.Contains("the antiforgery token", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            innerException = innerException.InnerException;
        }
        
        return false;
    }

    private bool IsDatabaseException(Exception ex)
    {
        // Check for SQL exceptions and timeout errors
        var exceptionType = ex.GetType().FullName ?? "";
        
        // Check current exception using case-insensitive comparison
        if (exceptionType.Contains("SqlException", StringComparison.OrdinalIgnoreCase) || 
            exceptionType.Contains("DbUpdateException", StringComparison.OrdinalIgnoreCase) ||
            (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) && ex.Message.Contains("sql", StringComparison.OrdinalIgnoreCase)) ||
            (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) && ex.Message.Contains("database", StringComparison.OrdinalIgnoreCase)) ||
            ex.Message.Contains("cannot open database", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check inner exceptions
        var innerException = ex.InnerException;
        while (innerException != null)
        {
            var innerType = innerException.GetType().FullName ?? "";
            
            if (innerType.Contains("SqlException", StringComparison.OrdinalIgnoreCase) || 
                innerType.Contains("DbUpdateException", StringComparison.OrdinalIgnoreCase) ||
                (innerException.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) && innerException.Message.Contains("sql", StringComparison.OrdinalIgnoreCase)) ||
                (innerException.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) && innerException.Message.Contains("database", StringComparison.OrdinalIgnoreCase)) ||
                innerException.Message.Contains("cannot open database", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            innerException = innerException.InnerException;
        }
        
        return false;
    }

    private void LogExceptionDetails(Exception ex, HttpContext context)
    {
        // Log request details
        _logger.LogError(
            "Request Details - Scheme: {Scheme}, Host: {Host}, QueryString: {QueryString}, ContentType: {ContentType}, ContentLength: {ContentLength}",
            context.Request.Scheme,
            context.Request.Host,
            context.Request.QueryString,
            context.Request.ContentType,
            context.Request.ContentLength);

        // Log user information if available
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            _logger.LogError(
                "User Context - Name: {UserName}, AuthenticationType: {AuthType}",
                context.User.Identity.Name,
                context.User.Identity.AuthenticationType);
        }

        // Log inner exceptions recursively
        var currentException = ex;
        var depth = 0;
        while (currentException.InnerException != null && depth < 5)
        {
            currentException = currentException.InnerException;
            depth++;
            _logger.LogError(
                "Inner Exception (Depth {Depth}): {ExceptionType}: {Message}",
                depth,
                currentException.GetType().Name,
                currentException.Message);
        }

        // Log aggregate exceptions specifically
        if (ex is AggregateException aggregateException)
        {
            _logger.LogError(
                "AggregateException contains {Count} inner exceptions",
                aggregateException.InnerExceptions.Count);

            for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
            {
                _logger.LogError(
                    "AggregateException[{Index}]: {ExceptionType}: {Message}",
                    i,
                    aggregateException.InnerExceptions[i].GetType().Name,
                    aggregateException.InnerExceptions[i].Message);
            }
        }
    }
}

/// <summary>
/// Extension method to register the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
