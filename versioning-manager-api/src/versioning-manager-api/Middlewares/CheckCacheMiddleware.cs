using Microsoft.Extensions.Caching.Memory;
using versioning_manager_api.Extensions;
using versioning_manager_api.Models;

namespace versioning_manager_api.Middlewares;

public class CheckCacheMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var username = ctx.User.GetUserName();
            var sessionId = ctx.User.GetSessionId();
            if (username == null || sessionId == null || !ctx.RequestServices.GetRequiredService<IMemoryCache>()
                    .TryGetValue<TokenResponseModel>(sessionId, out _))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next.Invoke(ctx);
    }
}

public static class CheckCacheMiddlewareExtensions
{
    public static IServiceCollection AddCheckCacheMiddleware(this IServiceCollection sc)
    {
        return sc.AddSingleton<CheckCacheMiddleware>();
    }

    public static IApplicationBuilder UseCheckCacheMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CheckCacheMiddleware>();
    }
}