using Microsoft.AspNetCore.Routing.Patterns;

namespace OrkinosaiCMS.Web.Middleware;

/// <summary>
/// Middleware to log endpoint routing decisions for diagnosing routing issues
/// Logs which endpoint was selected for each request
/// </summary>
public class EndpointRoutingLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EndpointRoutingLoggingMiddleware> _logger;

    public EndpointRoutingLoggingMiddleware(
        RequestDelegate next,
        ILogger<EndpointRoutingLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log BEFORE endpoint is selected
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        
        await _next(context);
        
        // Log AFTER endpoint is selected
        var endpoint = context.GetEndpoint();
        
        if (endpoint != null)
        {
            var endpointName = endpoint.DisplayName ?? endpoint.ToString() ?? "Unknown";
            var routePattern = endpoint.Metadata.GetMetadata<RoutePattern>();
            
            // Log at different levels based on the endpoint type
            if (endpointName.Contains("Fallback", StringComparison.OrdinalIgnoreCase))
            {
                // Log fallback routes as warnings for /admin/* paths (should not happen)
                if (requestPath.StartsWithSegments("/admin"))
                {
                    _logger.LogWarning(
                        "ROUTING ISSUE: {Method} {Path} matched FALLBACK endpoint: {Endpoint} - This should match Blazor route!",
                        requestMethod, requestPath, endpointName);
                }
                else
                {
                    _logger.LogInformation(
                        "Routing: {Method} {Path} → Fallback: {Endpoint}",
                        requestMethod, requestPath, endpointName);
                }
            }
            else if (endpointName.Contains("Blazor", StringComparison.OrdinalIgnoreCase) || 
                     routePattern?.RawText?.Contains("admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogInformation(
                    "Routing: {Method} {Path} → Blazor: {Endpoint}",
                    requestMethod, requestPath, endpointName);
            }
            else if (requestPath.StartsWithSegments("/api"))
            {
                _logger.LogInformation(
                    "Routing: {Method} {Path} → API: {Endpoint}",
                    requestMethod, requestPath, endpointName);
            }
            else
            {
                _logger.LogDebug(
                    "Routing: {Method} {Path} → {Endpoint}",
                    requestMethod, requestPath, endpointName);
            }
        }
        else
        {
            // No endpoint matched - this is unusual
            _logger.LogWarning(
                "Routing: {Method} {Path} → NO ENDPOINT MATCHED (this may indicate routing misconfiguration)",
                requestMethod, requestPath);
        }
    }
}

/// <summary>
/// Extension method to register the endpoint routing logging middleware
/// </summary>
public static class EndpointRoutingLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseEndpointRoutingLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<EndpointRoutingLoggingMiddleware>();
    }
}
