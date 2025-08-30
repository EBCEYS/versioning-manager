using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Projects;
using versioning_manager_api.Models.Responses.Projects;
using static versioning_manager_api.Routes.ControllerRoutes.ProjectAdministrationV1Routes;

namespace versioning_manager_api.Client.Services;

internal class ProjectAdministrationClientV1 : ClientBase, IProjectAdministrationClientV1
{
    public ProjectAdministrationClientV1(string serverAddress, TimeSpan? timeout) : base(
        new FlurlClient(serverAddress),
        timeout ?? TimeSpan.FromSeconds(10), JsonSerializerOptions.Web)
    {
        BaseUrl = BaseUrl.AppendPathSegment(GetBaseRoute());
    }

    public ProjectAdministrationClientV1(HttpClient client) : base(new FlurlClient(client), client.Timeout,
        JsonSerializerOptions.Web)
    {
        BaseUrl = BaseUrl.AppendPathSegment(GetBaseRoute());
    }

    public async Task CreateProjectAsync(CreateProjectModel request, string jwt, CancellationToken token = default)
    {
        await PostJsonAsync<CreateProjectModel, ProblemDetails>(
            url => url.AppendPathSegment(PostProjectRoute), request,
            GetJwtHeaders(jwt), token);
    }

    public async Task CreateProjectEntryAsync(CreateProjectEntryModel request, string jwt,
        CancellationToken token = default)
    {
        await PostJsonAsync<CreateProjectEntryModel, ProblemDetails>(
            url => url.AppendPathSegment(PostProjectEntryRoute), request,
            GetJwtHeaders(jwt), token);
    }

    public async Task<IReadOnlyCollection<ProjectInfoResponse>> GetAllProjectsAsync(string jwt,
        CancellationToken token = default)
    {
        return await GetJsonAsync<ProjectInfoResponse[], ProblemDetails>(
            url => url.AppendPathSegment(GetProjectsRoute),
            GetJwtHeaders(jwt), token);
    }

    public async Task<IReadOnlyCollection<ProjectEntryInfoResponse>> GetProjectEntriesAsync(string projectName,
        ProjectEntrySearchTypes searchType, string jwt,
        CancellationToken token = default)
    {
        return await GetJsonAsync<ProjectEntryInfoResponse[], ProblemDetails>(
            url => url.AppendPathSegment(
                    GetProjectEntriesRouteWith(projectName))
                .AppendQueryParam("searchType", searchType),
            GetJwtHeaders(jwt), token);
    }

    public async Task ChangeProjectEntryActualityAsync(int projectEntryId, bool newStatus, string jwt,
        CancellationToken token = default)
    {
        await PutAsync<ProblemDetails>(
            url => url.AppendPathSegment(
                    ChangeProjectEntryActualityRouteWith(projectEntryId))
                .AppendQueryParam("newStatus", newStatus), GetJwtHeaders(jwt), token);
    }

    public async Task<IReadOnlyCollection<ImageInfoResponse>> GetImagesAsync(int projectEntryId, string jwt,
        CancellationToken token = default)
    {
        return await GetJsonAsync<ImageInfoResponse[], ProblemDetails>(
            url => url.AppendPathSegment(
                GetProjectEntryImagesRouteWith(projectEntryId)),
            GetJwtHeaders(jwt), token);
    }

    public async Task CopyImagesToProjectAsync(int projectEntryId, IEnumerable<int> imageIds, string jwt,
        CancellationToken token = default)
    {
        await PostJsonAsync<IEnumerable<int>, ProblemDetails>(
            url => url.AppendPathSegment(
                MigrateImagesToAnotherProjectRouteWith(projectEntryId)),
            imageIds, GetJwtHeaders(jwt), token);
    }

    public async Task ChangeImageActivityAsync(int imageId, bool newState, string jwt,
        CancellationToken token = default)
    {
        await PutAsync<ProblemDetails>(
            url => url.AppendPathSegment(
                    ChangeImageActualityRouteWith(imageId))
                .AppendQueryParam("newState", newState), GetJwtHeaders(jwt), token);
    }
}