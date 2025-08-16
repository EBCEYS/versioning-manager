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
    ///     The device administration client. The jwt require.
    ///     <br />
    ///     Client for device manipulation. Create, delete etc.
    /// </summary>
    public IDeviceAdministrationClientV1 DeviceAdministrationClient { get; } =
        new DeviceAdministrationClientV1(serverAddress, timeouts?.DeviceAdministrationClientTimeout);

    //public IProjectAdministrationClient ProjectAdministrationClient { get; }
    /// <summary>
    ///     The project client. The api key require.
    ///     <br />
    ///     Client to manipulate with project's content by ServiceUploader.
    /// </summary>
    public IProjectClientV1? ProjectClient { get; set; } =
        new ProjectClientV1(serverAddress, timeouts?.ProjectClientTimout);
    //public IUsersClient UsersClient { get; }
}

/// <summary>
///     The <see cref="ClientsTimeouts" /> record.
/// </summary>
/// <param name="DeviceAdministrationClientTimeout">The timeout for <see cref="IDeviceAdministrationClientV1" />.</param>
/// <param name="ProjectClientTimout">The timeout for <see cref="IProjectClientV1" />.</param>
public record ClientsTimeouts(TimeSpan? DeviceAdministrationClientTimeout = null, TimeSpan? ProjectClientTimout = null);