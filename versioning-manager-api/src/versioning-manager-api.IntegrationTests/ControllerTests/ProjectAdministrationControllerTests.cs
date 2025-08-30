using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Projects;
using versioning_manager_api.Models.Requests.Users;

namespace versioning_manager_api.IntegrationTests.ControllerTests;

public class ProjectAdministrationControllerTests : IntegrationTestBase
{
    private IProjectAdministrationClientV1 _client;
    private string _validToken;

    private readonly CreateProjectModel _createProjectModel = new()
    {
        Name = "TestProject".ToLower(),
        AvailableSources = ["some"]
    };

    [SetUp]
    public async Task SetUp()
    {
        await ResetAsync();
        _client = Client.ProjectAdministrationClient;
        await DbContext.ProjectEntries.AsQueryable().ExecuteDeleteAsync();
        await DbContext.Projects.AsQueryable().ExecuteDeleteAsync();
        
        _validToken = (await Client.UsersClient.LoginAsync(new UserLoginModel
        {
            Username = TestsContext.DefaultUsername,
            Password = TestsContext.DefaultPassword
        })).Token;
    }

    [Test]
    public async Task When_CreateProjectTwice_Result_FirstSuccessAndSecondConflict()
    {
        await _client.CreateProjectAsync(_createProjectModel, _validToken);

        var act = () => _client.CreateProjectAsync(_createProjectModel, _validToken);

        (await act.Should().ThrowAsync<VersioningManagerApiException<ProblemDetails>>()).And.StatusCode.Should()
            .Be(StatusCodes.Status409Conflict);
    }

    [Test]
    public async Task When_CreateProjectEntryTwice_Result_FirstSuccessAndSecondConflict()
    {
        var entryRequest = new CreateProjectEntryModel
        {
            DefaultActuality = true,
            ProjectName = _createProjectModel.Name,
            Version = "some"
        };
        
        await _client.CreateProjectAsync(_createProjectModel, _validToken);

        await _client.CreateProjectEntryAsync(entryRequest, _validToken);
        
        var act = () => _client.CreateProjectEntryAsync(entryRequest, _validToken);
        
        (await act.Should().ThrowAsync<VersioningManagerApiException<ProblemDetails>>()).And.StatusCode.Should()
            .Be(StatusCodes.Status409Conflict);
    }
    
    [Test]
    public async Task When_CreateProjectEntry_Result_NotFound()
    {
        var entryRequest = new CreateProjectEntryModel
        {
            DefaultActuality = true,
            ProjectName = _createProjectModel.Name,
            Version = "some"
        };
        
        var act = () => _client.CreateProjectEntryAsync(entryRequest, _validToken);
        
        (await act.Should().ThrowAsync<VersioningManagerApiException<ProblemDetails>>()).And.StatusCode.Should()
            .Be(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task When_GetAllProjectsTwice_Result_FirstEmptySecondNotEmpty()
    {
        var allProjectsFirst = await _client.GetAllProjectsAsync(_validToken);

        await _client.CreateProjectAsync(_createProjectModel, _validToken);
        
        var allProjectsSecond = await _client.GetAllProjectsAsync(_validToken);
        
        allProjectsFirst.Should().BeEmpty();
        allProjectsSecond.Should().NotBeEmpty();
        allProjectsSecond.First().Name.Should().Be(_createProjectModel.Name);
    }

    [Test] 
    public async Task When_GetAllProjectEntriesAllAndActual_Result_FirstEmptySecondNotEmpty()
    {
        var entryRequest1 = new CreateProjectEntryModel
        {
            DefaultActuality = true,
            ProjectName = _createProjectModel.Name,
            Version = "some"
        };
        var entryRequest2 = new CreateProjectEntryModel
        {
            DefaultActuality = false,
            ProjectName = _createProjectModel.Name,
            Version = "some2"
        };
        await _client.CreateProjectAsync(_createProjectModel, _validToken);
        
        var allProjectEntriesFirst = await _client.GetProjectEntriesAsync(_createProjectModel.Name, ProjectEntrySearchTypes.All, _validToken);
        
        await _client.CreateProjectEntryAsync(entryRequest1, _validToken);
        await _client.CreateProjectEntryAsync(entryRequest2, _validToken);
        
        var allProjectEntriesSecond = await _client.GetProjectEntriesAsync(_createProjectModel.Name, ProjectEntrySearchTypes.All, _validToken);
        var actualProjectEntries = await _client.GetProjectEntriesAsync(_createProjectModel.Name, ProjectEntrySearchTypes.Actual, _validToken);
        
        allProjectEntriesFirst.Should().BeEmpty();
        allProjectEntriesSecond.Should().NotBeEmpty().And.HaveCount(2);
        actualProjectEntries.Should().NotBeEmpty().And.HaveCount(1);
    }

    [Test]
    public async Task When_ChangeProjectEntryActuality_Result_Success()
    {
        var entryRequest = new CreateProjectEntryModel
        {
            DefaultActuality = false,
            ProjectName = _createProjectModel.Name,
            Version = "some"
        };
        await _client.CreateProjectAsync(_createProjectModel, _validToken);
        await _client.CreateProjectEntryAsync(entryRequest, _validToken);

        var actualFirst =
            await _client.GetProjectEntriesAsync(_createProjectModel.Name, ProjectEntrySearchTypes.Actual, _validToken);
        var all = await _client.GetProjectEntriesAsync(_createProjectModel.Name, ProjectEntrySearchTypes.All, _validToken);
        var entry = all.First();
        await _client.ChangeProjectEntryActualityAsync(entry.Id, true, _validToken);
        
        var actual = await _client.GetProjectEntriesAsync(_createProjectModel.Name, ProjectEntrySearchTypes.Actual, _validToken);

        actualFirst.Should().BeEmpty();
        actual.Should().NotBeEmpty();
    }
}