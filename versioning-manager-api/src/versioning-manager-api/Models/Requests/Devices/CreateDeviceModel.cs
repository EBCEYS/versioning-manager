using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

namespace versioning_manager_api.Models.Requests.Devices;

/// <summary>
/// Create device api model.
/// </summary>
public class CreateDeviceModel
{
    /// <summary>
    /// The target source.
    /// </summary>
    /// <example>github.com</example>
    [MaxLength(FieldsLimits.MaxSourceName)]
    public required string Source { get; init; }
    /// <summary>
    /// The api key time to live.
    /// </summary>
    public DateTimeOffset ExpiresUtc { get; init; }
}