using versioning_manager_api.DbContext.DevDatabase;

namespace versioning_manager_api.Models.Responses.Projects;

/// <summary>
///     The image info api response.
/// </summary>
public class ImageInfoResponse
{
    /// <summary>
    ///     The id.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    ///     The service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    ///     The version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    ///     The tag.
    /// </summary>
    public required string Tag { get; init; }

    /// <summary>
    ///     The creator.
    /// </summary>
    public required Guid Creator { get; init; }

    /// <summary>
    ///     Is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    ///     Creation UTC.
    /// </summary>
    public required DateTimeOffset CreationUtc { get; init; }

    /// <summary>
    ///     Creates an instance of <see cref="ImageInfoResponse" /> from <see cref="DbImageInfo" />.
    /// </summary>
    /// <param name="dbImageInfo">The database image info.</param>
    /// <returns></returns>
    public static ImageInfoResponse Create(DbImageInfo dbImageInfo)
    {
        return new ImageInfoResponse
        {
            Id = dbImageInfo.Id,
            ServiceName = dbImageInfo.ServiceName,
            Version = dbImageInfo.Version,
            Tag = dbImageInfo.ImageTag,
            Creator = dbImageInfo.CreatorId,
            IsActive = dbImageInfo.IsActive,
            CreationUtc = dbImageInfo.CreationUTC
        };
    }
}