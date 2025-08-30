namespace versioning_manager_api.SystemObjects.Options;

/// <summary>
///     The docker client options.
/// </summary>
public class DockerClientOptions
{
    /// <summary>
    ///     Use default connection. <see cref="DockerHost" /> will be ignored.
    /// </summary>
    public required bool UseDefaultConnection { get; init; }

    /// <summary>
    ///     The docker host. Will be ignored if <see cref="UseDefaultConnection" /> is <c>true</c>.
    /// </summary>
    public string? DockerHost { get; init; }

    /// <summary>
    ///     The connection timeout.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(10.0);

    /// <summary>
    ///     The docker connection credentials.
    /// </summary>
    public DockerClientCredentials? Credentials { get; init; }
}

/// <summary>
///     The docker client credentials.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class DockerClientCredentials
{
    /// <summary>
    ///     The username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    ///     The password.
    /// </summary>
    public required string PasswordFile { get; init; }

    /// <summary>
    ///     Use tls. Default - <c>false</c>.
    /// </summary>
    public bool UseTls { get; init; } = false;
}