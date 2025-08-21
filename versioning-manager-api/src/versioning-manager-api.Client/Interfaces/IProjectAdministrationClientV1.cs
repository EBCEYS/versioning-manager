using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Projects;
using versioning_manager_api.Models.Responses.Projects;

namespace versioning_manager_api.Client.Interfaces;

/// <summary>
///     The <see cref="IProjectAdministrationClientV1" /> interface.
/// </summary>
public interface IProjectAdministrationClientV1
{
    /// <summary>
    ///     Creates the project.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task CreateProjectAsync(CreateProjectModel request, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Creates the project entry.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task CreateProjectEntryAsync(CreateProjectEntryModel request, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets all projects.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<ProjectInfoResponse>> GetAllProjectsAsync(string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets all project entries with specified <see cref="ProjectEntrySearchTypes" />.
    /// </summary>
    /// <param name="projectName">The project name.</param>
    /// <param name="searchType">The search type.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<ProjectEntryInfoResponse>> GetProjectEntriesAsync(string projectName,
        ProjectEntrySearchTypes searchType, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Changes the project entry actuality.
    /// </summary>
    /// <param name="projectEntryId">The project entry id.</param>
    /// <param name="newStatus">The new status. true - active; false - inactive.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task ChangeProjectEntryActualityAsync(int projectEntryId, bool newStatus, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets the project entry image infos.
    /// </summary>
    /// <param name="projectEntryId">The project entry id.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<ImageInfoResponse>> GetImagesAsync(int projectEntryId, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Copies the images to another project entry.
    /// </summary>
    /// <param name="projectEntryId">The project entry id.</param>
    /// <param name="imageIds">The image ids.</param>
    /// <param name="jwt">The cancellation token.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task CopyImagesToProjectAsync(int projectEntryId, IEnumerable<int> imageIds, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Changes image activity.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <param name="newState">The new state. true - active; false - inactive.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task ChangeImageActivityAsync(int imageId, bool newState, string jwt,
        CancellationToken token = default);
}