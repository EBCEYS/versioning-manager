namespace ServiceUploader.Models;

/// <summary>
/// The project info api response.
/// </summary>
public class DeviceProjectInfoResponse
{
    /// <summary>
    /// The project name.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// The actual entries.
    /// </summary>
    public required IEnumerable<DeviceProjectEntryInfo> ActualEntries { get; init; }

    public override string ToString()
    {
        return Name;
    }
}
/// <summary>
/// The project entry info.
/// </summary>
public class DeviceProjectEntryInfo
{
    /// <summary>
    /// The id.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// The version.
    /// </summary>
    public required string Version { get; init; }
    /// <summary>
    /// The images.
    /// </summary>
    public required IEnumerable<DeviceImageInfoResponse> Images { get; init; }

    public override string ToString()
    {
        return Version;
    }
}
/// <summary>
/// The image info response.
/// </summary>
public class DeviceImageInfoResponse
{
    /// <summary>
    /// The id.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// The tag.
    /// </summary>
    public required string Tag { get; init; }

    public override string ToString()
    {
        return Tag;
    }
}