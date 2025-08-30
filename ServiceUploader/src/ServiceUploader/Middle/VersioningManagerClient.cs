using ServiceUploader.Extensions;
using versioning_manager_api.Client;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;

namespace ServiceUploader.Middle;

internal class VersioningManagerClient
{
    private readonly IProjectClientV1 _client;
    private readonly string _token;

    public VersioningManagerClient(Uri url, string token, TimeSpan timeout)
    {
        var vmClient =
            new VersioningManagerApiClientV1(url.ToString(), new ClientsTimeouts(ProjectClientTimeout: timeout));
        _token = token;
        _client = vmClient.ProjectClient;
    }

    public async Task UploadImageAsync(UploadImageInfoModel model)
    {
        await _client.PostImageInfoAsync(model, _token);
    }

    public async Task DownloadImageAsync(int id, string destinationPath, Action<long, double> progressCallback,
        Action<long, double> finishCallback)
    {
        var stream = await _client.DownloadImageAsync(id, _token);
        await stream.DownloadFileAsync(destinationPath, progressCallback, finishCallback,
            TimeSpan.FromSeconds(1));
    }

    public async Task<DeviceProjectInfoResponse> GetProjectInfoAsync(string projectName)
    {
        return await _client.GetProjectInfoAsync(projectName, _token);
    }

    public async Task<Stream> GetProjectComposeAsync(int id)
    {
        return await _client.GetDockerComposeFileAsync(id, _token);
    }

    public async Task PostImageAsync(Stream fileStream)
    {
        await _client.UploadImageFileAsync(fileStream, _token);
    }
}