using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;

namespace versioning_manager_api.Client.Interfaces;

/// <summary>
///     The <see cref="IProjectClientV1" /> interface.
/// </summary>
public interface IProjectClientV1
{
    /// <summary>
    ///     Gets the project info.
    /// </summary>
    /// <param name="projectName">The project name.</param>
    /// <param name="apiKey">The api key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="DeviceProjectInfoResponse" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<DeviceProjectInfoResponse> GetProjectInfoAsync(string projectName, string apiKey,
        CancellationToken token = default);

    /// <summary>
    ///     Gets the image file.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <param name="apiKey">The api key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="Stream" /> with image file content.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<Stream> DownloadImageAsync(int imageId, string apiKey, CancellationToken token = default);

    /// <summary>
    ///     Gets the project info.
    /// </summary>
    /// <param name="imageInfo">The service image info.</param>
    /// <param name="apiKey">The api key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task PostImageInfoAsync(UploadImageInfoModel imageInfo, string apiKey, CancellationToken token = default);

    /// <summary>
    ///     Gets docker-compose file stream.
    /// </summary>
    /// <param name="entryId">The project entry id.</param>
    /// <param name="apiKey">The api key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="Stream" /> with docker-compose file content.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<Stream> GetDockerComposeFileAsync(int entryId, string apiKey, CancellationToken token = default);
}