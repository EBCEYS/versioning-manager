using versioning_manager_api.Client.Interfaces;

namespace versioning_manager_api.Client;

/// <summary>
///     The <see cref="ClientsTimeouts" /> record.
/// </summary>
/// <param name="DeviceAdministrationClientTimeout">The timeout for <see cref="IDeviceAdministrationClientV1" />.</param>
/// <param name="ProjectClientTimeout">The timeout for <see cref="IProjectClientV1" />.</param>
/// <param name="ProjectAdministrationClientTimeout">The timeout for <see cref="IProjectAdministrationClientV1" />.</param>
/// <param name="UsersClientTimeout">The timeout for <see cref="IUsersClientV1" />.</param>
public record ClientsTimeouts(
    TimeSpan? DeviceAdministrationClientTimeout = null,
    TimeSpan? ProjectClientTimeout = null,
    TimeSpan? ProjectAdministrationClientTimeout = null,
    TimeSpan? UsersClientTimeout = null);