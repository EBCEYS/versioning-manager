using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.Models.Responses.Devices;
using versioning_manager_api.Routes;

namespace versioning_manager_api.Client.Services;

internal class DeviceAdministrationClientV1(string serverAddress, TimeSpan? timeout) : ClientBase(
    new FlurlClient(serverAddress.AppendPathSegment(ControllerRoutes.DeviceAdministrationV1Routes.GetBaseRoute())),
    timeout ?? TimeSpan.FromSeconds(10), JsonSerializerOptions.Web), IDeviceAdministrationClientV1
{
    public async Task<DeviceTokenInfoResponse> CreateDeviceAsync(CreateDeviceModel request, string jwt,
        CancellationToken token = default)
    {
        return await PostJsonAsync<CreateDeviceModel, DeviceTokenInfoResponse, ProblemDetails>(
            url => url.AppendPathSegment(ControllerRoutes.DeviceAdministrationV1Routes.PostDeviceRoute), request,
            GetJwtHeaders(jwt), token);
    }

    public async Task<DeviceTokenInfoResponse> RefreshDeviceAsync(UpdateDeviceModel request, string jwt,
        CancellationToken token = default)
    {
        return await PutJsonAsync<UpdateDeviceModel, DeviceTokenInfoResponse, ProblemDetails>(
            url => url.AppendPathSegment(ControllerRoutes.DeviceAdministrationV1Routes.RefreshDeviceRoute), request,
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
        await DeleteJsonAsync<object, ProblemDetails>(
            url => url.AppendPathSegment(ControllerRoutes.DeviceAdministrationV1Routes.DeleteDeviceRoute)
                .AppendQueryParam("id", id), GetJwtHeaders(jwt), token);
    }

    private async Task<IReadOnlyCollection<DeviceInfoResponse>> GetDevicesAsync(string jwt,
        DeviceSearchType searchType = DeviceSearchType.Active, Guid? id = null, CancellationToken token = default)
    {
        return await GetJsonAsync<DeviceInfoResponse[], ProblemDetails>(url =>
        {
            url.AppendPathSegment(ControllerRoutes.DeviceAdministrationV1Routes.GetDevicesRoute)
                .AppendQueryParam("searchType", searchType.ToString());
            if (id != null) url.AppendQueryParam("id", id);
        }, GetJwtHeaders(jwt), token);
    }
}