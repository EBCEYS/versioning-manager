using System.Net;
using Docker.DotNet;
using Docker.DotNet.BasicAuth;
using Docker.DotNet.Models;
using Microsoft.Extensions.Options;
using versioning_manager_api.SystemObjects.Options;

namespace versioning_manager_api.Middle.Docker;

/// <summary>
///     The <see cref="IDockerController" /> interface.
/// </summary>
public interface IDockerController : IHostedService
{
    /// <summary>
    ///     Pulls the image from gitlab registry.
    /// </summary>
    /// <param name="imageName">The image tag.</param>
    /// <param name="token">The cancellation token.</param>
    Task PullImageFromGitlabAsync(string imageName,
        CancellationToken token = default);

    /// <summary>
    ///     Check image exists in local docker images storage.
    /// </summary>
    /// <param name="imageName">The image name.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns><c>true</c> if image exists; otherwise <c>false</c>.</returns>
    Task<bool> IsImageExistsAsync(string imageName, CancellationToken token = default);

    /// <summary>
    ///     Gets the image as file.
    /// </summary>
    /// <param name="imageName">The image tag.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A <see cref="Stream" /> with image archive file.</returns>
    Task<Stream> GetImageFileAsync(string imageName, CancellationToken token = default);

    /// <summary>
    ///     Removes the image from <see cref="DockerClient" /> registry.
    /// </summary>
    /// <param name="imageName">The image tag.</param>
    /// <param name="token">The cancellation token.</param>
    Task RemoveImageAsync(string imageName, CancellationToken token = default);

    /// <summary>
    ///     Gets the image list.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="ImagesListResponse" /> collection.</returns>
    Task<IList<ImagesListResponse>> GetImagesAsync(CancellationToken token = default);

    /// <summary>
    ///     Uploads the image to docker.
    /// </summary>
    /// <param name="imageStream">The image file stream.</param>
    /// <param name="token">The cancellation token.</param>
    Task UploadImageAsync(Stream imageStream, CancellationToken token = default);
}

/// <summary>
///     The docker controller.
/// </summary>
public class DockerController : IDockerController
{
    private readonly DockerClient client;
    private readonly AuthConfig gitAuth;

    /// <summary>
    ///     Initiates a new instance of <see cref="DockerController" />.
    /// </summary>
    /// <param name="opts">The docker client options.</param>
    /// <param name="gitOpts">The gitlab registry connection options.</param>
    public DockerController(IOptions<DockerClientOptions> opts, IOptions<GitlabRegistryConnectionOptions> gitOpts)
    {
        Credentials credentials = opts.Value.Credentials != null
            ? new BasicAuthCredentials(opts.Value.Credentials.Username,
                File.ReadAllText(opts.Value.Credentials.PasswordFile).Trim(),
                opts.Value.Credentials.UseTls)
            : new AnonymousCredentials();

        DockerClientConfiguration config = opts.Value.UseDefaultConnection
            ? new DockerClientConfiguration(credentials, opts.Value.ConnectionTimeout)
            : new DockerClientConfiguration(new Uri(opts.Value.DockerHost!), credentials, opts.Value.ConnectionTimeout);

        client = config.CreateClient();

        gitAuth = new AuthConfig
        {
            ServerAddress = gitOpts.Value.Address,
            Username = gitOpts.Value.Username,
            Password = File.ReadAllText(gitOpts.Value.KeyFile).Trim()
        };
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Pulls the image from gitlab registry.
    /// </summary>
    /// <param name="imageName">The image tag.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task PullImageFromGitlabAsync(string imageName,
        CancellationToken token = default)
    {
        await client.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = imageName
        }, gitAuth, new Progress<JSONMessage>(), token);
    }

    /// <summary>
    ///     Check image exists in local docker images storage.
    /// </summary>
    /// <param name="imageName">The image name.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns><c>true</c> if image exists; otherwise <c>false</c>.</returns>
    public async Task<bool> IsImageExistsAsync(string imageName, CancellationToken token = default)
    {
        try
        {
            await client.Images.InspectImageAsync(imageName, token);
            return true;
        }
        catch (DockerImageNotFoundException)
        {
            return false;
        }
        catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <summary>
    ///     Gets the image as file.
    /// </summary>
    /// <param name="imageName">The image tag.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A <see cref="Stream" /> with image archive file.</returns>
    public async Task<Stream> GetImageFileAsync(string imageName, CancellationToken token = default)
    {
        return await client.Images.SaveImageAsync(imageName, token);
    }

    /// <summary>
    ///     Removes the image from <see cref="DockerClient" /> registry.
    /// </summary>
    /// <param name="imageName">The image tag.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task RemoveImageAsync(string imageName, CancellationToken token = default)
    {
        await client.Images.DeleteImageAsync(imageName, new ImageDeleteParameters
        {
            Force = true
        }, token);
    }

    /// <summary>
    ///     Gets the image list.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="ImagesListResponse" /> collection.</returns>
    public Task<IList<ImagesListResponse>> GetImagesAsync(CancellationToken token = default)
    {
        return client.Images.ListImagesAsync(new ImagesListParameters
        {
            All = true
        }, token);
    }

    /// <summary>
    ///     Uploads the image to docker.
    /// </summary>
    /// <param name="imageStream">The image file stream.</param>
    /// <param name="token">The cancellation token.</param>
    public Task UploadImageAsync(Stream imageStream, CancellationToken token = default)
    {
        return client.Images.LoadImageAsync(new ImageLoadParameters(), imageStream, new Progress<JSONMessage>(), token);
    }
}

/// <summary>
///     The docker controller <see cref="IServiceCollection" /> exntensions.
/// </summary>
public static class DockerControllerServiceCollectionExtensions
{
    /// <summary>
    ///     Adds <see cref="DockerController" /> to <see cref="IServiceCollection" /> as <c>scoped</c> and
    ///     <see cref="IHostedService" />.
    /// </summary>
    /// <param name="sc">The service collection.</param>
    /// <returns>An instance of <paramref name="sc" />.</returns>
    public static IServiceCollection AddDockerController(this IServiceCollection sc)
    {
        return sc.AddSingleton<IDockerController, DockerController>()
            .AddHostedService(sp => sp.GetRequiredService<IDockerController>());
    }
}