using FluentAssertions;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Models.Requests.Users;

namespace versioning_manager_api.IntegrationTests.UsersControllerTests;

public class UsersControllerV1Tests : IntegrationTestBase
{
    private static IUsersClientV1 UsersClient => Client.UsersClient;

    [Test]
    public async Task When_Login_Result_Success()
    {
        var response = await UsersClient.LoginAsync(new UserLoginModel
        {
            Username = TestsContext.DefaultUsername,
            Password = TestsContext.DefaultPassword
        });

        response.Roles.Should().NotBeEmpty();
        response.TimeToLive.Should().NotBeNull();
        response.Token.Should().NotBeNullOrEmpty();
    }
}