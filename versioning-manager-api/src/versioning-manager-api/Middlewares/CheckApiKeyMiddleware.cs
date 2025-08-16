using Microsoft.Extensions.Primitives;
using versioning_manager_api.DevDatabase;
using versioning_manager_api.Middle.ApiKeyProcess;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.StaticStorages;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Middlewares;

/// <summary>
///     Check apikey middleware.
/// </summary>
public class CheckApiKeyMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        if (ctx.Request.Headers.TryGetValue(ApikeyStorage.ApikeyHeader, out StringValues key))
        {
            ILogger<CheckApiKeyMiddleware> logger =
                ctx.RequestServices.GetRequiredService<ILogger<CheckApiKeyMiddleware>>();
            using IDisposable? scope = logger.BeginScope("Request with api key");

            IApiKeyProcessor keyProcessor = ctx.RequestServices.GetRequiredService<IApiKeyProcessor>();
            string? firstEntryKey = key.FirstOrDefault();
            if (firstEntryKey == null)
            {
                logger.LogWarning("Unsuccessful api key decryption");
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            (ApiKeyValidationResult, ApiKeyEntity?) validationResult = await keyProcessor.ValidateAsync(firstEntryKey,
                ctx.RequestServices.GetRequiredService<VmDatabaseContext>(),
                ctx.RequestServices.GetRequiredService<IHashHelper>());
            if (validationResult.Item1 != ApiKeyValidationResult.Valid || validationResult.Item2 == null)
            {
                logger.LogWarning("Error {validationResult} while validating api key", validationResult);
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            ctx.Items[ApikeyStorage.ApikeyHeader] = validationResult.Item2;
            logger.LogInformation("Successfully validated api key");
        }

        await next.Invoke(ctx);
    }
}

/// <summary>
///     Check apikey middleware extensions.
/// </summary>
public static class CheckApiKeyMiddlewareExtensions
{
    /// <summary>
    ///     Adds <see cref="CheckApiKeyMiddleware" /> as scoped service.
    /// </summary>
    /// <param name="sc">The service collection.</param>
    /// <returns>The instance of <paramref name="sc" />.</returns>
    public static IServiceCollection AddApiCheckMiddleware(this IServiceCollection sc)
    {
        return sc.AddScoped<CheckApiKeyMiddleware>();
    }

    /// <summary>
    ///     Adds <see cref="CheckApiKeyMiddleware" /> to <paramref name="app" /> pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder" /> instance.</param>
    /// <returns>The instance of <paramref name="app" /></returns>
    public static IApplicationBuilder UseApiCheckMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CheckApiKeyMiddleware>();
    }
}