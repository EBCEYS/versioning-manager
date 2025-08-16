namespace versioning_manager_api.Models;

/// <summary>
///     The device search type.
/// </summary>
public enum DeviceSearchType
{
    /// <summary>
    ///     Get all devices.
    /// </summary>
    All,

    /// <summary>
    ///     Get only active devices.
    /// </summary>
    Active,

    /// <summary>
    ///     Get one device.
    /// </summary>
    One
}