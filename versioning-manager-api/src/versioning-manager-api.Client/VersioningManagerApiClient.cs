using versioning_manager_api.Client.Services;

namespace versioning_manager_api.Client;

internal class VersioningManagerApiClient
{
    public DeviceAdministrationClient DeviceAdministrationClient { get; }
    public ProjectAdministrationClient ProjectAdministrationClient { get; }
    public ProjectClient ProjectClient { get; }
    public UsersClient UsersClient { get; }
}