using System.Text.Json;

namespace versioning_manager_api.Client.Extensions;

/// <summary>
///     The <see cref="ObjectExtensions" /> static class.
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    ///     Serializes the <paramref name="obj" /> to json string.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The json as string.</returns>
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, JsonSerializerOptions.Web);
    }
}