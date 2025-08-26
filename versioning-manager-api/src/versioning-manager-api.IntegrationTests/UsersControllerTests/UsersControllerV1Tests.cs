using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Users;

namespace versioning_manager_api.IntegrationTests.UsersControllerTests;

[SingleThreaded]
public class UsersControllerV1Tests : IntegrationTestBase
{
    private IUsersClientV1 _client;
    private string _validToken;

    [SetUp]
    public async Task SetUp()
    {
        await ResetAsync();
        await DbContext.Users.Where(u => u.Username != TestsContext.DefaultUsername).ExecuteDeleteAsync();
        await DbContext.Roles.Where(r => r.Name != TestsContext.DefaultRole).ExecuteDeleteAsync();
        await Task.Delay(100);
        _client = Client.UsersClient;
        var validLoginResponse = await GetTokenAsync();
        _validToken = validLoginResponse.Token;
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

    [Test]
    public async Task When_Register_Result_Success()
    {
        const string username = "Vitaliy";
        const string password = "12345";

        await _client.CreateUserAsync(new UserCreationApiModel
        {
            Username = username,
            Password = password,
            Role = TestsContext.DefaultRole
        }, _validToken);

        var loginResponse = await _client.LoginAsync(new UserLoginModel
        {
            Username = username,
            Password = password
        });

        loginResponse.Token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task When_Register_Result_Failure()
    {
        const string username = "Vitaliy";
        const string password = "12345";

        try
        {
            await _client.CreateUserAsync(new UserCreationApiModel
            {
                Username = username,
                Password = password,
                Role = TestsContext.DefaultRole
            }, _validToken);
            await _client.CreateUserAsync(new UserCreationApiModel
            {
                Username = username,
                Password = password,
                Role = TestsContext.DefaultRole
            }, _validToken);
        }
        catch (VersioningManagerApiException<ProblemDetails> ex)
        {
            ex.StatusCode.Should().Be(409);
            ex.ErrorInfo.Should().NotBeNull();
            return;
        }

        Assert.Fail("No exception thrown!");
    }

    [Test]
    public async Task When_CreateRole_Result_Success()
    {
        const string newRole = "some_role";
        var systemRoles = await _client.GetSystemRolesAsync(_validToken);

        await _client.CreateRoleAsync(new CreateRoleModel
        {
            Name = newRole,
            Roles = systemRoles.ToArray()
        }, _validToken);

        var roles = await _client.GetUsersRolesAsync(_validToken);
        var newRoleEntity = roles.Roles.First(r => r.Name == newRole);

        systemRoles.Should().NotBeEmpty();

        newRoleEntity.Name.Should().Be(newRole);
        newRoleEntity.Roles.Should().BeEquivalentTo(systemRoles);
    }

    [Test]
    public async Task When_UpdateRole_Result_Success()
    {
        const string newRole = "some_role";
        var systemRoles = await _client.GetSystemRolesAsync(_validToken);

        await _client.CreateRoleAsync(new CreateRoleModel
        {
            Name = newRole,
            Roles = systemRoles.ToArray()
        }, _validToken);

        var newRoles = systemRoles.SkipLast(1).ToArray();
        await _client.UpdateRoleAsync(newRole, newRoles, _validToken);

        var roles = await _client.GetUsersRolesAsync(_validToken);
        var newRoleEntity = roles.Roles.First(r => r.Name == newRole);

        systemRoles.Should().NotBeEmpty();

        newRoleEntity.Name.Should().Be(newRole);
        newRoleEntity.Roles.Should().BeEquivalentTo(newRoles);
    }

    [Test]
    public async Task When_RemoveRole_Result_Success()
    {
        const string newRole = "some_role";

        var systemRoles = await _client.GetSystemRolesAsync(_validToken);

        await _client.CreateRoleAsync(new CreateRoleModel
        {
            Name = newRole,
            Roles = systemRoles.ToArray()
        }, _validToken);

        await _client.DeleteRoleAsync(newRole, _validToken);

        var roles = await _client.GetUsersRolesAsync(_validToken);
        var newRoleEntity = roles.Roles.FirstOrDefault(r => r.Name == newRole);

        newRoleEntity.Should().BeNull();
    }

    [Test]
    public async Task When_RemoveUser_Result_Success()
    {
        const string username = "Vitaliy";
        const string password = "12345";

        await _client.CreateUserAsync(new UserCreationApiModel
        {
            Username = username,
            Password = password,
            Role = TestsContext.DefaultRole
        }, _validToken);

        await _client.DeleteUserAsync(username, _validToken);

        var newUser = await _client.GetUserInfoAsync(username, _validToken);

        newUser.Should().NotBeNull();
        newUser.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task When_GetAllUsers_Result_Success()
    {
        const string username = "Vitaliy";
        const string password = "12345";

        await _client.CreateUserAsync(new UserCreationApiModel
        {
            Username = username,
            Password = password,
            Role = TestsContext.DefaultRole
        }, _validToken);

        var users = await _client.GetAllUsersAsync(_validToken);

        users.Should().NotBeEmpty();
        users.Count.Should().Be(2);
    }
}