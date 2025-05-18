using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using versioning_manager_api.Controllers;
using versioning_manager_api.Exceptions;
using versioning_manager_api.StaticStorages;

namespace versioning_manager_api.Attributes;

/// <summary>
/// The requirement apikey header attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RequireApiKeyAttribute : ExceptionFilterAttribute, IActionFilter
{
    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        Type currentController = context.Controller.GetType();
        if (IsDefined(currentController, GetType()) && !context.HttpContext.Request.Headers.TryGetValue(ApikeyStorage.ApikeyHeader, out _))
        {
            throw new ApiKeyRequireException(ApikeyStorage.ApikeyHeader);
        }
    }

    /// <inheritdoc />
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApiKeyRequireException ex) return;
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status403Forbidden,
            Detail = ex.Message
        };
        context.Result = new ApiKeyRequireExceptionResponse(details);
    }
}

/// <summary>
/// The apikey require exception response.
/// </summary>
public class ApiKeyRequireExceptionResponse : ObjectResult
{
    /// <inheritdoc />
    public ApiKeyRequireExceptionResponse(object? value) : base(value)
    {
        StatusCode = StatusCodes.Status403Forbidden;
        ContentTypes = [ "application/problem+json" ];
    }
}