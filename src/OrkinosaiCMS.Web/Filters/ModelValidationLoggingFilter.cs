using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrkinosaiCMS.Web.Filters;

/// <summary>
/// Action filter to log model validation errors
/// This helps identify HTTP 400 errors caused by invalid model binding
/// </summary>
public class ModelValidationLoggingFilter : IActionFilter
{
    private readonly ILogger<ModelValidationLoggingFilter> _logger;

    public ModelValidationLoggingFilter(ILogger<ModelValidationLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var traceId = context.HttpContext.TraceIdentifier;
        var actionName = context.ActionDescriptor.DisplayName;
        
        _logger.LogInformation(
            "Action executing [{TraceId}] - {ActionName} - ModelState.IsValid: {IsValid}",
            traceId,
            actionName,
            context.ModelState.IsValid);

        if (!context.ModelState.IsValid)
        {
            _logger.LogWarning(
                "Model validation failed [{TraceId}] - {ActionName} - Error count: {ErrorCount}",
                traceId,
                actionName,
                context.ModelState.ErrorCount);

            foreach (var key in context.ModelState.Keys)
            {
                var errors = context.ModelState[key]?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        _logger.LogWarning(
                            "Model validation error [{TraceId}] - Field: {Field}, Error: {Error}, Exception: {Exception}",
                            traceId,
                            key,
                            error.ErrorMessage,
                            error.Exception?.Message ?? "None");
                    }
                }
            }
        }

        // Log action parameters
        if (context.ActionArguments.Count > 0)
        {
            _logger.LogInformation(
                "Action parameters [{TraceId}] - {ActionName} - Parameter count: {ParamCount}",
                traceId,
                actionName,
                context.ActionArguments.Count);

            foreach (var param in context.ActionArguments)
            {
                _logger.LogInformation(
                    "Action parameter [{TraceId}] - Name: {ParamName}, Type: {ParamType}, Value: {ParamValue}",
                    traceId,
                    param.Key,
                    param.Value?.GetType().Name ?? "null",
                    param.Value?.ToString() ?? "null");
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var traceId = context.HttpContext.TraceIdentifier;
        var actionName = context.ActionDescriptor.DisplayName;
        
        if (context.Exception != null)
        {
            _logger.LogError(context.Exception,
                "Action threw exception [{TraceId}] - {ActionName} - Exception: {ExceptionType}",
                traceId,
                actionName,
                context.Exception.GetType().FullName);
        }
        else if (context.Result is BadRequestObjectResult badRequest)
        {
            _logger.LogWarning(
                "Action returned BadRequest [{TraceId}] - {ActionName} - Value: {Value}",
                traceId,
                actionName,
                badRequest.Value);
        }
        else if (context.Result is ObjectResult objectResult && objectResult.StatusCode >= 400)
        {
            _logger.LogWarning(
                "Action returned error status [{TraceId}] - {ActionName} - StatusCode: {StatusCode}, Value: {Value}",
                traceId,
                actionName,
                objectResult.StatusCode,
                objectResult.Value);
        }
        else
        {
            _logger.LogInformation(
                "Action executed [{TraceId}] - {ActionName} - Result type: {ResultType}",
                traceId,
                actionName,
                context.Result?.GetType().Name ?? "null");
        }
    }
}
