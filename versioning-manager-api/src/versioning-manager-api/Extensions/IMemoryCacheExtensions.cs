using Microsoft.Extensions.Caching.Memory;

namespace versioning_manager_api.Extensions;

public static class MemoryCacheExtensions
{
    public static T Set<T>(this IMemoryCache cache, string key, T value, TimeSpan? timeToLive)
    {
        if (timeToLive == null)
        {
            return cache.Set(key, value);
        }

        return cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeToLive.Value
        });
    }
}