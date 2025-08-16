namespace versioning_manager_api.Models.Responses.Devices;

/// <summary>
///     The device info api response.
/// </summary>
public class DeviceTokenInfoResponse
{
    /// <summary>
    ///     The device id.
    /// </summary>
    public Guid DeviceId { get; init; }

    /// <summary>
    ///     The api key.
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    ///     The source.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    ///     The expires.
    /// </summary>
    public DateTimeOffset? Expires { get; init; }
}