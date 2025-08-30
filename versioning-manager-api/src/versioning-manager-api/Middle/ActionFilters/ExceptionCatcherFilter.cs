using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using versioning_manager_api.Exceptions;

namespace versioning_manager_api.Middle.ActionFilters;

/// <inheritdoc />
public class ExceptionCatcherFilter(ILogger<ExceptionCatcherFilter> logger) : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiKeyRequireException) return;
        
        logger.LogError(context.Exception, "Error on request processing!");
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = context.Exception.Message,
            Instance = context.HttpContext.Request.Path,
            Title = "Internal error! See logs!"
        };
        context.Result = new ObjectResult(details)
        {
            ContentTypes = ["application/problem+json"],
            StatusCode = details.Status
        };
    }
}