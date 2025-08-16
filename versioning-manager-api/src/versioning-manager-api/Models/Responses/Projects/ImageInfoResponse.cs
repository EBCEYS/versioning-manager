using versioning_manager_api.DevDatabase;

namespace versioning_manager_api.Models.Responses.Projects;

/// <summary>
///     The image info api response.
/// </summary>
/// <param name="id">The image id.</param>
/// <param name="service">The service name.</param>
/// <param name="version">The image version.</param>
/// <param name="tag">The image tag.</param>
/// <param name="creator">The creator.</param>
public class ImageInfoResponse(
    int id,
    string service,
    string version,
    string tag,
    Guid creator,
    bool isActive,
    DateTimeOffset creationUtc)
{
    /// <summary>
    ///     The id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    ///     The service name.
    /// </summary>
    public string ServiceName { get; } = service;

    /// <summary>
    ///     The version.
    /// </summary>
    public string Version { get; } = version;

    /// <summary>
    ///     The tag.
    /// </summary>
    public string Tag { get; } = tag;

    /// <summary>
    ///     The creator.
    /// </summary>
    public Guid Creator { get; } = creator;

    /// <summary>
    ///     Is active.
    /// </summary>
    public bool IsActive { get; } = isActive;

    /// <summary>
    ///     Creation UTC.
    /// </summary>
    public DateTimeOffset CreationUtc { get; } = creationUtc;

    /// <summary>
    ///     Creates an instance of <see cref="ImageInfoResponse" /> from <see cref="DbImageInfo" />.
    /// </summary>
    /// <param name="dbImageInfo">The database image info.</param>
    /// <returns></returns>
    public static ImageInfoResponse Create(DbImageInfo dbImageInfo)
    {
        return new ImageInfoResponse(dbImageInfo.Id, dbImageInfo.ServiceName, dbImageInfo.Version, dbImageInfo.ImageTag,
            dbImageInfo.Creator.Id, dbImageInfo.IsActive, dbImageInfo.CreationUTC);
    }
}