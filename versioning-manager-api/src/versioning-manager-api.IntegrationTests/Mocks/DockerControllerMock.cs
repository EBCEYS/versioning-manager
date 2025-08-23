using Docker.DotNet.Models;
using versioning_manager_api.Middle.Docker;

namespace versioning_manager_api.IntegrationTests.Mocks;

public class DockerControllerMock : IDockerController
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task PullImageFromGitlabAsync(string imageName, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> IsImageExistsAsync(string imageName, CancellationToken token = default)
    {
        return Task.FromResult(true);
    }

    public Task<Stream> GetImageFileAsync(string imageName, CancellationToken token = default)
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task RemoveImageAsync(string imageName, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Task<IList<ImagesListResponse>> GetImagesAsync(CancellationToken token = default)
    {
        return Task.FromResult<IList<ImagesListResponse>>(new List<ImagesListResponse>());
    }

    public Task UploadImageAsync(Stream imageStream, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}