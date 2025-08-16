using System.Text.Json;
using System.Text.Json.Serialization;

namespace versioning_manager_api.SystemObjects;

/// <summary>
///     The apikey entity.
/// </summary>
public class ApiKeyEntity
{
    [JsonIgnore] private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    /// <summary>
    ///     The source.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    ///     The key expire datetime UTC.
    /// </summary>
    public DateTimeOffset ExpiresUtc { get; init; }

    /// <summary>
    ///     The device id.
    /// </summary>
    public Guid DeviceId { get; init; }

    /// <summary>
    ///     Serialize object to json.
    /// </summary>
    /// <returns>The json string.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOpts);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ApiKeyEntity" /> from <paramref name="json" />.
    /// </summary>
    /// <param name="json">The json string.</param>
    /// <returns>
    ///     A new instance of <see cref="ApiKeyEntity" /> created from <paramref name="json" />.<br />
    ///     <c>null</c> if caught any errors while deserializing.
    /// </returns>
    public static ApiKeyEntity? FromJson(string? json)
    {
        if (json == null) return null;
        try
        {
            return JsonSerializer.Deserialize<ApiKeyEntity>(json, JsonOpts);
        }
        catch (Exception)
        {
            return null;
        }
    }
}