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
            // Check if this is a database connectivity error
            var isDatabaseError = IsDatabaseException(ex);
            if (isDatabaseError)
            {
                context.Items["DatabaseError"] = true;
            }

            // Log the exception with full details
            _logger.LogError(ex, 
                "Unhandled exception occurred. Path: {Path}, Method: {Method}, StatusCode: {StatusCode}, TraceId: {TraceId}, IsDatabaseError: {IsDatabaseError}",
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode,
                context.TraceIdentifier,
                isDatabaseError);

            // Log additional context
            LogExceptionDetails(ex, context);

            // Re-throw to let ASP.NET Core's exception handler handle the response
            throw;
        }
    }

    private bool IsDatabaseException(Exception ex)
    {
        // Check for SQL exceptions and timeout errors
        var exceptionType = ex.GetType().FullName ?? "";
        var exceptionMessage = ex.Message.ToLower();
        
        // Check current exception
        if (exceptionType.Contains("SqlException") || 
            exceptionType.Contains("DbUpdateException") ||
            (exceptionMessage.Contains("timeout") && exceptionMessage.Contains("sql")) ||
            (exceptionMessage.Contains("connection") && exceptionMessage.Contains("database")) ||
            exceptionMessage.Contains("cannot open database"))
        {
            return true;
        }
        
        // Check inner exceptions
        var innerException = ex.InnerException;
        while (innerException != null)
        {
            var innerType = innerException.GetType().FullName ?? "";
            var innerMessage = innerException.Message.ToLower();
            
            if (innerType.Contains("SqlException") || 
                innerType.Contains("DbUpdateException") ||
                (innerMessage.Contains("timeout") && innerMessage.Contains("sql")) ||
                (innerMessage.Contains("connection") && innerMessage.Contains("database")) ||
                innerMessage.Contains("cannot open database"))
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
