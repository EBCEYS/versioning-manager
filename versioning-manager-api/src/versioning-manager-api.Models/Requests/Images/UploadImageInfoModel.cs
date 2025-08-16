namespace versioning_manager_api.Models.Requests.Images;

/// <summary>
///     The upload image info api model.
/// </summary>
public class UploadImageInfoModel
{
    /// <summary>
    ///     The project name.
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    ///     The service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    ///     The image tag.
    /// </summary>
    public required string ImageTag { get; init; }

    /// <summary>
    ///     The image version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    ///     The docker compose file content.
    /// </summary>
    public required string DockerCompose { get; init; }
}