using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace versioning_manager_api.Models.Requests.Devices;

/// <summary>
///     The update device api model.
/// </summary>
public class UpdateDeviceModel
{
    /// <summary>
    ///     The device key.
    /// </summary>
    public Guid DeviceKey { get; init; }

    /// <summary>
    ///     The new apikey time to live.
    /// </summary>
    public DateTimeOffset ExpiresUtc { get; init; }

    /// <summary>
    ///     The source.
    /// </summary>
    [MaxLength(FieldsLimits.MaxSourceName)]
    public required string Source { get; init; }
}