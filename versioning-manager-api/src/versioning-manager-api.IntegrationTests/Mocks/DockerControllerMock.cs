using System.Text;
using Docker.DotNet.Models;
using versioning_manager_api.Middle.Docker;

namespace versioning_manager_api.IntegrationTests.Mocks;

public class DockerControllerMock : IDockerController
{
    public static readonly Dictionary<string, Stream> Images = [];

    public static async Task WriteImageAsync(string tag, string content)
    {
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(), leaveOpen: true);
        await writer.WriteLineAsync(content);
        stream.Seek(0, SeekOrigin.Begin);
        Images.TryAdd(tag, stream);
        
    }
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
        if (Images.TryGetValue(imageName, out var imageStream))
        {
            return Task.FromResult(imageStream);
        }
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