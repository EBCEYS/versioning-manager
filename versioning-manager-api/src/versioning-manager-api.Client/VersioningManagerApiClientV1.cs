using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Client.Services;

namespace versioning_manager_api.Client;

/// <summary>
///     The <see cref="VersioningManagerApiClientV1" /> class.
/// </summary>
/// <param name="serverAddress">The server address.</param>
/// <param name="timeouts">The timeouts.</param>
public class VersioningManagerApiClientV1(string serverAddress, ClientsTimeouts? timeouts = null)
{
    /// <summary>
    ///     The base service address.
    /// </summary>
    public string BaseAddress { get; init; } = serverAddress;

    /// <summary>
    ///     The device administration client. The jwt require.
    ///     <br />
    ///     Client for device manipulation. Create, delete etc.
    /// </summary>
    public IDeviceAdministrationClientV1 DeviceAdministrationClient { get; } =
        new DeviceAdministrationClientV1(serverAddress, timeouts?.DeviceAdministrationClientTimeout);

    /// <summary>
    ///     The project administration client. The jwt require.
    ///     <br />
    ///     Client for projects, entries and images manipulation.
    /// </summary>
    public IProjectAdministrationClientV1 ProjectAdministrationClient { get; } =
        new ProjectAdministrationClientV1(serverAddress, timeouts?.ProjectAdministrationClientTimeout);

    /// <summary>
    ///     The project client. The api key require.
    ///     <br />
    ///     Client to manipulate with project's content by ServiceUploader.
    /// </summary>
    public IProjectClientV1 ProjectClient { get; } =
        new ProjectClientV1(serverAddress, timeouts?.ProjectClientTimeout);

    /// <summary>
    ///     The users' client. The jwt require for all methods except login.
    ///     <br />
    ///     Client for users and roles manipulations.
    /// </summary>
    public IUsersClientV1 UsersClient { get; } =
        new UsersClientV1(serverAddress, timeouts?.UsersClientTimeout);
}