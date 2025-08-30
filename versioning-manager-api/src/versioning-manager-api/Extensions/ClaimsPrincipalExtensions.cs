using System.Security.Claims;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace versioning_manager_api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserName(this ClaimsPrincipal? user)
    {
        return user?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? GetSessionId(this ClaimsPrincipal? user)
    {
        return user?.FindFirstValue(ClaimTypes.Sid);
    }
}