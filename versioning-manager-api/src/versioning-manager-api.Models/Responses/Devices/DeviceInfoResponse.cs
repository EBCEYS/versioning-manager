// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace versioning_manager_api.Models.Responses.Devices;

/// <summary>
///     The device info api response.
/// </summary>
public class DeviceInfoResponse
{
    /// <summary>
    ///     The id.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     The token expires UTC.
    /// </summary>
    public DateTimeOffset ExpiresUtc { get; init; }

    /// <summary>
    ///     Is active.
    /// </summary>
    public bool IsActive { get; init; }
}