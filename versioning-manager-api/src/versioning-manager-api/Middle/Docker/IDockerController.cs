using Docker.DotNet;
using Docker.DotNet.Models;

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