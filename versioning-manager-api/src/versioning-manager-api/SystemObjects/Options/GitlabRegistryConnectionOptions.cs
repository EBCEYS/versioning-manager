namespace versioning_manager_api.SystemObjects.Options;

/// <summary>
/// Gitlab registry connection options.
/// </summary>
public class GitlabRegistryConnectionOptions
{
    /// <summary>
    /// The registry address.
    /// </summary>
    public required string Address { get; init; }
    /// <summary>
    /// The username.
    /// </summary>
    public required string Username { get; init; }
    /// <summary>
    /// The registry access key.
    /// </summary>
    public required string KeyFile { get; init; }
}