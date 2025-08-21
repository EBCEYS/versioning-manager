using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Users;
using versioning_manager_api.Models.Responses.Users;

namespace versioning_manager_api.Client.Interfaces;

/// <summary>
///     The <see cref="IUsersClientV1" /> interface.
/// </summary>
public interface IUsersClientV1
{
    /// <summary>
    ///     Creates the user.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task CreateUserAsync(UserCreationApiModel request, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Logins at the system.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="TokenResponseModel" /> if login successfully; otherwise throws ex.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<TokenResponseModel> LoginAsync(UserLoginModel request, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Creates new role.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task CreateRoleAsync(CreateRoleModel request, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Changes user password.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task ChangePasswordAsync(string username, ChangePasswordModel request, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Changes self user password.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task ChangeSelfPasswordAsync(ChangePasswordModel request, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets the system roles.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The collection of system roles.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<string>> GetSystemRolesAsync(string jwt, CancellationToken token = default);

    /// <summary>
    ///     Gets the users roles.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The collection of users roles.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<string>> GetUsersRolesAsync(string jwt, CancellationToken token = default);

    /// <summary>
    ///     Changes the user role.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="roleName">the role name.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task ChangeUserRoleAsync(string username, string roleName, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Deletes the role.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task DeleteRoleAsync(string role, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Updates the role.
    /// </summary>
    /// <param name="role">The role to update.</param>
    /// <param name="newRoles">The new roles.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task UpdateRoleAsync(string role, IEnumerable<string> newRoles, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Deletes the user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task DeleteUserAsync(string username, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Activates the deleted user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task SetUserActiveAsync(string username, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Gets all users.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The collection of <see cref="UserInfoResponseModel" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<UserInfoResponseModel>> GetAllUsersAsync(string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets all active users.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The collection of <see cref="UserInfoResponseModel" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<UserInfoResponseModel>> GetActiveUsersAsync(string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets user info.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="UserInfoResponseModel" /> if user exists; otherwise null.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<UserInfoResponseModel?> GetUserInfoAsync(string username, string jwt,
        CancellationToken token = default);
}