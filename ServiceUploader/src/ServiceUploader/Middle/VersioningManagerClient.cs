using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Extensions;
using ServiceUploader.Models;

namespace ServiceUploader.Middle;

internal class VersioningManagerClient : IDisposable
{
    private readonly HttpClient client;
    private readonly Uri baseUri;

    public VersioningManagerClient(Uri url, string token, TimeSpan timeout)
    {
        baseUri = url;
        client = new HttpClient()
        {
            Timeout = timeout
        };
        client.DefaultRequestHeaders.Add("ApiKey", token);
    }

    public async Task UploadImageAsync(UploadImageInfoModel model)
    {
        Uri url = new(baseUri, "version-manager/api/project/image");
        HttpResponseMessage response =
            await client.PostAsJsonAsync(url, model, RequestSerializerContext.Default.UploadImageInfoModel);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<Stream> GetImageAsync(int id)
    {
        Uri url = new(baseUri, "version-manager/api/project/image/file");
        QueryBuilder qBuilder = new() { { "id", id.ToString() } };
        UriBuilder builder = new(url)
        {
            Query = qBuilder.ToQueryString().ToString()
        };
        HttpResponseMessage response = await client.GetAsync(builder.ToString());
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync();
        }

        throw new HttpRequestException(await response.Content.ReadAsStringAsync());
    }

    public async Task<DeviceProjectInfoResponse> GetProjectInfoAsync(string projectName)
    {
        Uri url = new(baseUri, $"version-manager/api/project/project/{WebUtility.UrlEncode(projectName)}/info");
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DeviceProjectInfoResponse>(RequestSerializerContext.Default
                       .DeviceProjectInfoResponse) ??
                   throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        throw new HttpRequestException(await response.Content.ReadAsStringAsync());
    }

    public async Task<Stream> GetProjectComposeAsync(int id)
    {
        Uri url = new(baseUri, $"version-manager/api/project/project/{id}/compose");
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync();
        }
        throw new HttpRequestException(await response.Content.ReadAsStringAsync());
    }

    public void Dispose()
    {
        client.Dispose();
    }
}