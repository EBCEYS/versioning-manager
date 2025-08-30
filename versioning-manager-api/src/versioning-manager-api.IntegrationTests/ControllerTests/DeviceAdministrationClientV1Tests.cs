using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using versioning_manager_api.Client.Interfaces;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.Models.Requests.Users;

namespace versioning_manager_api.IntegrationTests.ControllerTests;

public class DeviceAdministrationClientV1Tests : IntegrationTestBase
{
    private IDeviceAdministrationClientV1 _client;
    private string _validToken;

    [SetUp]
    public async Task Setup()
    {
        await ResetAsync();
        _validToken = (await Client.UsersClient.LoginAsync(new UserLoginModel
        {
            Username = TestsContext.DefaultUsername,
            Password = TestsContext.DefaultPassword
        })).Token;
        _client = Client.DeviceAdministrationClient;

        await DbContext.Devices.AsQueryable().ExecuteDeleteAsync();
    }

    [Test]
    public async Task When_CreateDevice_Result_Success()
    {
        var device = await _client.CreateDeviceAsync(new CreateDeviceModel
        {
            Source = "test_source",
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
        }, _validToken);

        device.ApiKey.Should().NotBeNullOrWhiteSpace();
        device.Expires.Should().BeAfter(DateTimeOffset.UtcNow);
        device.Source.Should().NotBeNullOrWhiteSpace();
        device.DeviceId.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task When_UpdateDevice_Result_Success()
    {
        var device = await _client.CreateDeviceAsync(new CreateDeviceModel
        {
            Source = "test_source",
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
        }, _validToken);

        var refreshed = await _client.RefreshDeviceAsync(new UpdateDeviceModel
        {
            DeviceKey = device.DeviceId,
            ExpiresUtc = device.Expires!.Value.AddHours(1),
            Source = "some_source"
        }, _validToken);

        refreshed.ApiKey.Should().NotBeNullOrWhiteSpace();
        refreshed.Source.Should().NotBeNullOrWhiteSpace();
        refreshed.DeviceId.Should().NotBe(Guid.Empty);

        refreshed.Source.Should().NotBe(device.Source);
        refreshed.ApiKey.Should().NotBe(device.ApiKey);
        refreshed.Expires.Should().BeAfter(device.Expires.Value);
    }

    [Test]
    public async Task When_DeleteDeviceAndGetActive_Result_Success()
    {
        var device = await _client.CreateDeviceAsync(new CreateDeviceModel
        {
            Source = "test_source",
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
        }, _validToken);

        await _client.DeleteDeviceAsync(device.DeviceId, _validToken);

        var getDevice = await _client.GetDeviceAsync(device.DeviceId, _validToken);
        var activeDevices = await _client.GetActiveDevicesAsync(_validToken);

        getDevice!.IsActive.Should().BeFalse();
        activeDevices.Should().BeEmpty();
    }

    [TestCase(5)]
    public async Task When_GetDevices_Result_ExpectedNum(byte num)
    {
        var deviceId = Guid.Empty;
        for (var i = 0; i < num; i++)
        {
            var device = await _client.CreateDeviceAsync(new CreateDeviceModel
            {
                Source = "test_source",
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            }, _validToken);
            deviceId = device.DeviceId;
        }

        await _client.DeleteDeviceAsync(deviceId, _validToken);

        var getDevices = await _client.GetAllDevicesAsync(_validToken);
        getDevices.Should().HaveCount(num);
    }
}