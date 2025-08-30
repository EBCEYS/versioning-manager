using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Client.Services;

namespace versioning_manager_api.Client;

/// <summary>
///     The <see cref="VersioningManagerApiClientV1" /> class.
/// </summary>
public class VersioningManagerApiClientV1
{
    /// <summary>
    ///     Initiates the new instance of <see cref="VersioningManagerApiClientV1" />.
    /// </summary>
    /// <param name="serverAddress">The server address.</param>
    /// <param name="timeouts">The timeouts.</param>
    public VersioningManagerApiClientV1(string serverAddress, ClientsTimeouts? timeouts = null)
    {
        BaseAddress = serverAddress;
        DeviceAdministrationClient =
            new DeviceAdministrationClientV1(serverAddress, timeouts?.DeviceAdministrationClientTimeout);
        ProjectAdministrationClient =
            new ProjectAdministrationClientV1(serverAddress, timeouts?.ProjectAdministrationClientTimeout);
        ProjectClient = new ProjectClientV1(serverAddress, timeouts?.ProjectClientTimeout);
        UsersClient = new UsersClientV1(serverAddress, timeouts?.UsersClientTimeout);
    }

    /// <summary>
    ///     Initiates the new instance of <see cref="VersioningManagerApiClientV1" />.
    ///     <br />
    ///     It's better to use another ctor.
    /// </summary>
    /// <param name="client">The http client.</param>
    public VersioningManagerApiClientV1(HttpClient client)
    {
        BaseAddress = client.BaseAddress?.ToString() ?? "";
        DeviceAdministrationClient = new DeviceAdministrationClientV1(client);
        ProjectAdministrationClient = new ProjectAdministrationClientV1(client);
        ProjectClient = new ProjectClientV1(client);
        UsersClient = new UsersClientV1(client);
    }

    /// <summary>
    ///     The base service address.
    /// </summary>
    public string BaseAddress { get; init; }

    /// <summary>
    ///     The device administration client. The jwt require.
    ///     <br />
    ///     Client for device manipulation. Create, delete etc.
    /// </summary>
    public IDeviceAdministrationClientV1 DeviceAdministrationClient { get; }

    /// <summary>
    ///     The project administration client. The jwt require.
    ///     <br />
    ///     Client for projects, entries and images manipulation.
    /// </summary>
    public IProjectAdministrationClientV1 ProjectAdministrationClient { get; }

    /// <summary>
    ///     The project client. The api key require.
    ///     <br />
    ///     Client to manipulate with project's content by ServiceUploader.
    /// </summary>
    public IProjectClientV1 ProjectClient { get; }

    /// <summary>
    ///     The users' client. The jwt require for all methods except login.
    ///     <br />
    ///     Client for users and roles manipulations.
    /// </summary>
    public IUsersClientV1 UsersClient { get; }
}