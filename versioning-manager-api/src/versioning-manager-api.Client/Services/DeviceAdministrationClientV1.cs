using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.Models.Responses.Devices;
using static versioning_manager_api.Routes.ControllerRoutes.DeviceAdministrationV1Routes;

namespace versioning_manager_api.Client.Services;

internal class DeviceAdministrationClientV1 : ClientBase, IDeviceAdministrationClientV1
{
    public DeviceAdministrationClientV1(string serverAddress, TimeSpan? timeout) : base(
        new FlurlClient(serverAddress),
        timeout ?? TimeSpan.FromSeconds(10), JsonSerializerOptions.Web)
    {
        BaseUrl = BaseUrl.AppendPathSegment(GetBaseRoute());
    }

    public DeviceAdministrationClientV1(HttpClient client) : base(new FlurlClient(client), client.Timeout,
        JsonSerializerOptions.Web)
    {
        BaseUrl = BaseUrl.AppendPathSegment(GetBaseRoute());
    }

    public async Task<DeviceTokenInfoResponse> CreateDeviceAsync(CreateDeviceModel request, string jwt,
        CancellationToken token = default)
    {
        return await PostJsonAsync<CreateDeviceModel, DeviceTokenInfoResponse, ProblemDetails>(
            url => url.AppendPathSegment(PostDeviceRoute), request,
            GetJwtHeaders(jwt), token);
    }

    public async Task<DeviceTokenInfoResponse> RefreshDeviceAsync(UpdateDeviceModel request, string jwt,
        CancellationToken token = default)
    {
        return await PutJsonAsync<UpdateDeviceModel, DeviceTokenInfoResponse, ProblemDetails>(
            url => url.AppendPathSegment(RefreshDeviceRoute), request,
            GetJwtHeaders(jwt), token);
    }

    public Task<IReadOnlyCollection<DeviceInfoResponse>> GetAllDevicesAsync(string jwt,
        CancellationToken token = default)
    {
        return GetDevicesAsync(jwt, DeviceSearchType.All, token: token);
    }

    public Task<IReadOnlyCollection<DeviceInfoResponse>> GetActiveDevicesAsync(string jwt,
        CancellationToken token = default)
    {
        return GetDevicesAsync(jwt, token: token);
    }

    public async Task<DeviceInfoResponse?> GetDeviceAsync(Guid id, string jwt, CancellationToken token = default)
    {
        return (await GetDevicesAsync(jwt, DeviceSearchType.One, id, token)).FirstOrDefault();
    }

    public async Task DeleteDeviceAsync(Guid id, string jwt, CancellationToken token = default)
    {
        await DeleteAsync<ProblemDetails>(
            url => url.AppendPathSegment(DeleteDeviceRoute)
                .AppendQueryParam("id", id), GetJwtHeaders(jwt), token);
    }

    private async Task<IReadOnlyCollection<DeviceInfoResponse>> GetDevicesAsync(string jwt,
        DeviceSearchType searchType = DeviceSearchType.Active, Guid? id = null, CancellationToken token = default)
    {
        return await GetJsonAsync<DeviceInfoResponse[], ProblemDetails>(url =>
        {
            url.AppendPathSegment(GetDevicesRoute)
                .AppendQueryParam("searchType", searchType.ToString());
            if (id != null) url.AppendQueryParam("id", id);
        }, GetJwtHeaders(jwt), token);
    }
}