using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Users;
using versioning_manager_api.Models.Responses.Users;
using static versioning_manager_api.Routes.ControllerRoutes.UsersV1Routes;

namespace versioning_manager_api.Client.Services;

internal class UsersClientV1(string serverAddress, TimeSpan? timeout) : ClientBase(
    new FlurlClient(serverAddress.AppendPathSegment(GetBaseRoute())),
    timeout ?? TimeSpan.FromSeconds(10), JsonSerializerOptions.Web), IUsersClientV1
{
    public async Task CreateUserAsync(UserCreationApiModel request, string jwt, CancellationToken token = default)
    {
        await PostJsonAsync<UserCreationApiModel, ProblemDetails>(
            url => url.AppendPathSegment(CreateNewUserRoute), request,
            GetJwtHeaders(jwt), token);
    }

    public async Task<TokenResponseModel> LoginAsync(UserLoginModel request, string jwt,
        CancellationToken token = default)
    {
        return await PostJsonAsync<UserLoginModel, TokenResponseModel, ProblemDetails>(
            url => url.AppendPathSegment(LoginRoute), request, GetJwtHeaders(jwt),
            token);
    }

    public async Task CreateRoleAsync(CreateRoleModel request, string jwt, CancellationToken token = default)
    {
        await PostJsonAsync<CreateRoleModel, ProblemDetails>(url => url.AppendPathSegment(CreateRoleRoute),
            request, GetJwtHeaders(jwt), token);
    }

    public async Task ChangePasswordAsync(string username, ChangePasswordModel request, string jwt,
        CancellationToken token = default)
    {
        await PutJsonAsync<ChangePasswordModel, ProblemDetails>(
            url => url.AppendPathSegment(ChangePasswordRoute).AppendQueryParam("username", username), request,
            GetJwtHeaders(jwt), token);
    }

    public async Task ChangeSelfPasswordAsync(ChangePasswordModel request, string jwt,
        CancellationToken token = default)
    {
        await PutJsonAsync<ChangePasswordModel, ProblemDetails>(url => url.AppendPathSegment(ChangeSelfPasswordRoute),
            request, GetJwtHeaders(jwt), token);
    }

    public async Task<IReadOnlyCollection<string>> GetSystemRolesAsync(string jwt, CancellationToken token = default)
    {
        return await GetJsonAsync<string[], ProblemDetails>(url => url.AppendPathSegment(GetSystemRolesRoute),
            GetJwtHeaders(jwt), token);
    }

    public async Task<IReadOnlyCollection<string>> GetUsersRolesAsync(string jwt, CancellationToken token = default)
    {
        return await GetJsonAsync<string[], ProblemDetails>(url => url.AppendPathSegment(GetUserRolesRoute),
            GetJwtHeaders(jwt), token);
    }

    public async Task ChangeUserRoleAsync(string username, string roleName, string jwt,
        CancellationToken token = default)
    {
        await PutAsync<ProblemDetails>(
            url => url.AppendPathSegment(ChangeUsersRoleRoute).AppendQueryParam("username", username)
                .AppendQueryParam("roleName", roleName), GetJwtHeaders(jwt), token);
    }

    public async Task DeleteRoleAsync(string role, string jwt, CancellationToken token = default)
    {
        await DeleteAsync<ProblemDetails>(
            url => url.AppendPathSegment(DeleteUserRoleRoute).AppendQueryParam("role", role), GetJwtHeaders(jwt),
            token);
    }

    public async Task UpdateRoleAsync(string role, IEnumerable<string> newRoles, string jwt,
        CancellationToken token = default)
    {
        await PutJsonAsync<IEnumerable<string>, ProblemDetails>(
            url => url.AppendPathSegment(UpdateRoleRoute).AppendQueryParam("role", role), newRoles, GetJwtHeaders(jwt),
            token);
    }

    public async Task DeleteUserAsync(string username, string jwt, CancellationToken token = default)
    {
        await DeleteAsync<ProblemDetails>(
            url => url.AppendPathSegment(DeleteUserRoute).AppendQueryParam("username", username), GetJwtHeaders(jwt),
            token);
    }

    public async Task SetUserActiveAsync(string username, string jwt, CancellationToken token = default)
    {
        await PutAsync<ProblemDetails>(
            url => url.AppendPathSegment(ActivateDeletedUserRoute).AppendQueryParam("username", username),
            GetJwtHeaders(jwt), token);
    }

    public Task<IReadOnlyCollection<UserInfoResponseModel>> GetAllUsersAsync(string jwt,
        CancellationToken token = default)
    {
        return GetUsersAsync(UsersSearchType.All, null, jwt, token);
    }

    public Task<IReadOnlyCollection<UserInfoResponseModel>> GetActiveUsersAsync(string jwt,
        CancellationToken token = default)
    {
        return GetUsersAsync(UsersSearchType.ActiveOnly, null, jwt, token);
    }

    public async Task<UserInfoResponseModel?> GetUserInfoAsync(string username, string jwt,
        CancellationToken token = default)
    {
        return (await GetUsersAsync(UsersSearchType.One, username, jwt, token)).FirstOrDefault();
    }

    private async Task<IReadOnlyCollection<UserInfoResponseModel>> GetUsersAsync(UsersSearchType searchType,
        string? username, string jwt,
        CancellationToken token = default)
    {
        return await GetJsonAsync<UserInfoResponseModel[], ProblemDetails>(url =>
        {
            url.AppendPathSegment(GetUserInfoRoute)
                .AppendQueryParam("searchType", searchType);
            if (username != null) url.AppendQueryParam("username", username);
        }, GetJwtHeaders(jwt), token);
    }
}