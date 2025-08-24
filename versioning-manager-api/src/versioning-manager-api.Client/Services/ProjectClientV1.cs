using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;
using versioning_manager_api.Routes.StaticStorages;
using static versioning_manager_api.Routes.ControllerRoutes.ProjectV1Routes;

namespace versioning_manager_api.Client.Services;

internal class ProjectClientV1 : ClientBase, IProjectClientV1
{
    public ProjectClientV1(string serverAddress, TimeSpan? timeout) : base(
        new FlurlClient(serverAddress.AppendPathSegments(GetBaseRoute())),
        timeout ?? TimeSpan.FromSeconds(120), JsonSerializerOptions.Web)
    {
    }

    public ProjectClientV1(HttpClient client) : base(new FlurlClient(client), client.Timeout, JsonSerializerOptions.Web)
    {
        BaseUrl = BaseUrl.AppendPathSegment(GetBaseRoute());
    }

    public async Task<DeviceProjectInfoResponse> GetProjectInfoAsync(string projectName, string apiKey,
        CancellationToken token = default)
    {
        return await GetJsonAsync<DeviceProjectInfoResponse, ProblemDetails>(url =>
                url.AppendPathSegments(GetProjectInfoRouteWith(projectName)),
            GetHeaders(apiKey),
            token);
    }

    public async Task<Stream> DownloadImageAsync(int imageId, string apiKey, CancellationToken token = default)
    {
        return await GetStreamAsync<ProblemDetails>(
            url => url.AppendPathSegment(DownloadImageRoute)
                .AppendQueryParam("id", imageId), GetHeaders(apiKey), token);
    }

    public async Task PostImageInfoAsync(UploadImageInfoModel imageInfo, string apiKey,
        CancellationToken token = default)
    {
        await PostJsonAsync<UploadImageInfoModel, object, ProblemDetails>(
            url => url.AppendPathSegment(PostImageInfoRoute), imageInfo,
            GetHeaders(apiKey),
            token);
    }

    public async Task<Stream> GetDockerComposeFileAsync(int entryId, string apiKey, CancellationToken token = default)
    {
        return await GetStreamAsync<ProblemDetails>(
            url => url.AppendPathSegment(GetProjectComposeFileRouteWith(entryId)),
            GetHeaders(apiKey), token);
    }

    public Task UploadImageFileAsync(Stream imageStream, string apiKey, CancellationToken token = default)
    {
        return PostStreamAsync<ProblemDetails>(url => url.AppendPathSegment(UploadImageRoute), imageStream,
            GetHeaders(apiKey), token);
    }

    private static Dictionary<string, object> GetHeaders(string apiKey)
    {
        return new Dictionary<string, object>(GetDefaultHeaders())
        {
            { ApikeyStorage.ApikeyHeader, apiKey }
        };
    }
}