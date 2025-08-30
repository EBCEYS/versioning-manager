using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Requests.Projects;
using versioning_manager_api.Models.Requests.Users;
using versioning_manager_api.Models.Responses.Devices;

namespace versioning_manager_api.IntegrationTests.ControllerTests;

public class ProjectControllerTests : IntegrationTestBase
{
    private IProjectClientV1 _client;
    private IDeviceAdministrationClientV1 _deviceClient;
    private IProjectAdministrationClientV1 _projectClient;
    private string _validToken;
    private DeviceTokenInfoResponse _deviceInfo;

    private const string ProjectName = "some_project";
    private const string Source = "source";

    [SetUp]
    public async Task Setup()
    {
        await ResetAsync();
        _client = Client.ProjectClient;
        _deviceClient = Client.DeviceAdministrationClient;
        _projectClient = Client.ProjectAdministrationClient;

        await DbContext.Projects.AsQueryable().ExecuteDeleteAsync();
        await DbContext.ProjectEntries.AsQueryable().ExecuteDeleteAsync();
        await DbContext.Images.AsQueryable().ExecuteDeleteAsync();
        await DbContext.Devices.AsQueryable().ExecuteDeleteAsync();

        _validToken = (await Client.UsersClient.LoginAsync(new UserLoginModel
        {
            Username = TestsContext.DefaultUsername,
            Password = TestsContext.DefaultPassword
        })).Token;

        _deviceInfo = await CreateDeviceAsync();

        await CreateProjectAsync();
    }

    private async Task CreateProjectAsync()
    {
        await _projectClient.CreateProjectAsync(new CreateProjectModel
        {
            Name = ProjectName,
            AvailableSources = [Source]
        }, _validToken);
        await _projectClient.CreateProjectEntryAsync(new CreateProjectEntryModel
        {
            ProjectName = ProjectName,
            DefaultActuality = true,
            Version = "some"
        }, _validToken);
    }

    private async Task<DeviceTokenInfoResponse> CreateDeviceAsync()
    {
        return await _deviceClient.CreateDeviceAsync(new CreateDeviceModel
        {
            Source = Source,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5)
        }, _validToken);
    }

    [Test]
    public async Task When_UploadImageInfo_Result_Success()
    {
        await _client.PostImageInfoAsync(new UploadImageInfoModel
        {
            DockerCompose = "services:",
            ImageTag = "some/image:latest",
            ProjectName = ProjectName,
            ServiceName = "some-service",
            Version = "latest"
        }, _deviceInfo.ApiKey);

        var projInfo = await _client.GetProjectInfoAsync(ProjectName, _deviceInfo.ApiKey);

        projInfo.ActualEntries.Should().NotBeEmpty();
        projInfo.ActualEntries.First().Images.Should().NotBeEmpty();
    }

    [Test]
    public async Task When_UploadImageInfo_Result_InvalidToken()
    {
        var act = () => _client.PostImageInfoAsync(new UploadImageInfoModel
        {
            DockerCompose = "services:",
            ImageTag = "some/image:latest",
            ProjectName = ProjectName,
            ServiceName = "some-service",
            Version = "latest"
        }, "VitaliksKey");

        (await act.Should().ThrowAsync<VersioningManagerApiException<ProblemDetails>>()).And.StatusCode.Should()
            .Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task When_GetProjectImages_Result_Success()
    {
        var creationRequest = new UploadImageInfoModel
        {
            DockerCompose = "services:",
            ImageTag = "some/image:latest",
            ProjectName = ProjectName,
            ServiceName = "some-service",
            Version = "latest"
        };
        await _client.PostImageInfoAsync(creationRequest, _deviceInfo.ApiKey);

        var entries =
            await _projectClient.GetProjectEntriesAsync(ProjectName, ProjectEntrySearchTypes.Actual, _validToken);
        var entry = entries.First();
        var images = await _projectClient.GetImagesAsync(entry.Id, _validToken);

        images.Should().NotBeEmpty();
        images.Should().HaveCount(1);
        images.First().ServiceName.Should().Be(creationRequest.ServiceName);
        images.First().Version.Should().Be(creationRequest.Version);
        images.First().Tag.Should().Be(creationRequest.ImageTag);
        images.First().IsActive.Should().Be(true);
    }

    [Test]
    public async Task When_MigrateImageToAnotherEntry_Result_Success()
    {
        var creationRequest = new UploadImageInfoModel
        {
            DockerCompose = "services:",
            ImageTag = "some/image:latest",
            ProjectName = ProjectName,
            ServiceName = "some-service",
            Version = "latest"
        };
        await _client.PostImageInfoAsync(creationRequest, _deviceInfo.ApiKey);
        
        await _projectClient.CreateProjectEntryAsync(new CreateProjectEntryModel
        {
            ProjectName = ProjectName,
            Version = "some-version2",
            DefaultActuality = true
        }, _validToken);

        var entries =
            await _projectClient.GetProjectEntriesAsync(ProjectName, ProjectEntrySearchTypes.All, _validToken);

        var oldEntry = entries.First(e => !e.IsActual);
        var newEntry = entries.First(e => e.IsActual);

        var images = await _projectClient.GetImagesAsync(oldEntry.Id, _validToken);

        await _projectClient.CopyImagesToProjectAsync(newEntry.Id, images.Select(i => i.Id).ToArray(), _validToken);
        
        var migratedImages = await _projectClient.GetImagesAsync(newEntry.Id, _validToken);
        
        migratedImages.Should().NotBeEmpty();
    }

    [Test]
    public async Task When_ChangeImageActuality_Result_Success()
    {
        var creationRequest = new UploadImageInfoModel
        {
            DockerCompose = "services:",
            ImageTag = "some/image:latest",
            ProjectName = ProjectName,
            ServiceName = "some-service",
            Version = "latest"
        };
        await _client.PostImageInfoAsync(creationRequest, _deviceInfo.ApiKey);
        
        var entry =
            (await _projectClient.GetProjectEntriesAsync(ProjectName, ProjectEntrySearchTypes.Actual, _validToken)).First();
        var images = await _projectClient.GetImagesAsync(entry.Id, _validToken);
        await _projectClient.ChangeImageActivityAsync(images.First().Id, false, _validToken);
        
        var newImages = (await _projectClient.GetImagesAsync(entry.Id, _validToken)).Where(i => i.IsActive).ToArray();
        newImages.Should().BeEmpty();
    }
}