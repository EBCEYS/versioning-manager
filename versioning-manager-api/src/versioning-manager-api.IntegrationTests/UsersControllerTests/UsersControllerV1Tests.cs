using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Users;

namespace versioning_manager_api.IntegrationTests.UsersControllerTests;

public class UsersControllerV1Tests : IntegrationTestBase
{
    private IUsersClientV1 _client;

    [SetUp]
    public async Task SetUp()
    {
        await ResetAsync();
        _client = Client.UsersClient;
    }

    private Task<TokenResponseModel> GetTokenAsync()
    {
        return _client.LoginAsync(GetValidLoginRequest());
    }
    private static UserLoginModel GetValidLoginRequest()
    {
        return new UserLoginModel
        {
            Username = TestsContext.DefaultUsername,
            Password = TestsContext.DefaultPassword
        };
    }

    [Test]
    public async Task When_Login_Result_Success()
    {
        var response = await GetTokenAsync();

        response.Roles.Should().NotBeEmpty();
        response.TimeToLive.Should().NotBeNull();
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task When_Login_Result_Failure()
    {
        try
        {
            await _client.LoginAsync(new UserLoginModel
            {
                Username = "Vitaliy",
                Password = "12345"
            });
        }
        catch (VersioningManagerApiException<ProblemDetails> ex)
        {
            ex.StatusCode.Should().Be(404);
            ex.ErrorInfo.Should().NotBeNull();
            return;
        }

        Assert.Fail("No exception thrown!");
    }
}