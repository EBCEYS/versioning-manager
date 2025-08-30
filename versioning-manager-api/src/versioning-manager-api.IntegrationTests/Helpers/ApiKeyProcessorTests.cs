using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.IntegrationTests.Mocks;
using versioning_manager_api.IntegrationTests.TestData;
using versioning_manager_api.Middle.ApiKeyProcess;
using versioning_manager_api.Middle.CryptsProcess;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.SystemObjects.Options;

namespace versioning_manager_api.IntegrationTests.Helpers;

public class ApiKeyProcessorTests : IntegrationTestBase
{
    private const string CryptKeyFilePath = FilesCreator.CryptKeyFilePath;
    private const string CryptIvFilePath = FilesCreator.CryptIvFilePath;
    private const string Prefix = "abc_";

    private ApiKeyProcessor _apiKeyProcessor;
    private VmDatabaseContext _dbCotext;
    private HashHelper _hashHelper;

    [SetUp]
    public async Task Initialize()
    {
        await ResetAsync();
        _dbCotext = DbContext;
        await _dbCotext.Devices.AsQueryable().ExecuteDeleteAsync();
        OptionsWrapper<ApiKeyOptions> options = new(new ApiKeyOptions
        {
            CryptKeyFilePath = CryptKeyFilePath,
            CryptIVFilePath = CryptIvFilePath,
            Prefix = Prefix
        });
        ICryptHelper cryptHelper = new CryptHelper(options);
        _apiKeyProcessor = new ApiKeyProcessor(cryptHelper);

        _hashHelper = new HashHelper();
    }

    [TestCaseSource(nameof(GetDataForTests))]
    public void When_SomeEntityKeyed_With_SpecifiedParams_Result_Success(Guid id, string source, DateTimeOffset expires)
    {
        var apiKey = _apiKeyProcessor.Generate(id, source, expires);

        var entity = _apiKeyProcessor.Decrypt(apiKey);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        entity.Should().NotBeNull();
        entity.DeviceId.Should().Be(id);
        entity.Source.Should().Be(source);
        entity.ExpiresUtc.Should().Be(expires);
    }

    private static IEnumerable<TestCaseData> GetDataForTests()
    {
        yield return new TestCaseData(Guid.NewGuid(), "abs", DateTimeOffset.MaxValue);
        yield return new TestCaseData(Guid.NewGuid(), "abs1", DateTimeOffset.MaxValue);
        yield return new TestCaseData(Guid.NewGuid(), "abs2", DateTimeOffset.MaxValue);
        yield return new TestCaseData(Guid.NewGuid(), "abs3", DateTimeOffset.MaxValue);
        yield return new TestCaseData(Guid.NewGuid(), "abs4", DateTimeOffset.MaxValue);
    }

    [TestCaseSource(nameof(GetValidDataForValidation))]
    public async Task When_ValidateSomeEntity_With_SpecifiedParams_Result_Valid(Guid id, string source,
        DateTimeOffset expires)
    {
        var apiKey = _apiKeyProcessor.Generate(id, source, expires);

        var salt = _hashHelper.GenerateSalt();

        await AddDeviceWithUser(id, source, expires, salt, apiKey);

        var result =
            await _apiKeyProcessor.ValidateAsync(apiKey, _dbCotext, _hashHelper);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        result.Item1.Should().Be(ApiKeyValidationResult.Valid);
        result.Item2.Should().NotBeNull();
    }

    private static IEnumerable<TestCaseData> GetValidDataForValidation()
    {
        yield return new TestCaseData(Guid.NewGuid(), "SomeSource", DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(1)));
        yield return new TestCaseData(Guid.NewGuid(), "SomeSource", DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(2)));
        yield return new TestCaseData(Guid.NewGuid(), "SomeSource", DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(3)));
    }

    [TestCaseSource(nameof(GetInValidDataForValidation))]
    public async Task When_ValidateSomeEntity_With_SpecifiedParams_Result_InValid(Guid id, string source,
        DateTimeOffset expires)
    {
        var apiKey = _apiKeyProcessor.Generate(id, source, expires);

        var salt = _hashHelper.GenerateSalt();

        await AddDeviceWithUser(id, source, expires, salt, apiKey);

        var result =
            await _apiKeyProcessor.ValidateAsync(apiKey, _dbCotext, _hashHelper);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        result.Item1.Should().Be(ApiKeyValidationResult.Expired);
        result.Item2.Should().NotBeNull();
    }

    private static IEnumerable<TestCaseData> GetInValidDataForValidation()
    {
        yield return new TestCaseData(Guid.NewGuid(), "SomeSource",
            DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(-1)));
        yield return new TestCaseData(Guid.NewGuid(), "SomeSource",
            DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(-2)));
        yield return new TestCaseData(Guid.NewGuid(), "SomeSource",
            DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(-3)));
    }

    [Test]
    public async Task When_ValidateEntity_With_WrongSource_Result_Invalid()
    {
        var id = Guid.NewGuid();
        var source = "someSource";
        var expires = DateTimeOffset.Now;
        var apiKey = _apiKeyProcessor.Generate(id, source, expires);

        var salt = _hashHelper.GenerateSalt();

        await AddDeviceWithUser(id, "AnotherSalt", expires, salt, apiKey);

        var result =
            await _apiKeyProcessor.ValidateAsync(apiKey, _dbCotext, _hashHelper);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        result.Item1.Should().Be(ApiKeyValidationResult.IncorrectSource);
        result.Item2.Should().NotBeNull();
    }

    [Test]
    public async Task When_ValidateEntity_With_WrongApikey_Result_Invalid()
    {
        var id = Guid.NewGuid();
        var source = "someSource";
        var expires = DateTimeOffset.Now;
        var apiKey = _apiKeyProcessor.Generate(id, source, expires) + "some string";

        var salt = _hashHelper.GenerateSalt();

        await AddDeviceWithUser(id, source, expires, salt, apiKey);

        var result =
            await _apiKeyProcessor.ValidateAsync(apiKey, _dbCotext, _hashHelper);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        result.Item1.Should().Be(ApiKeyValidationResult.IncorrectKey);
        result.Item2.Should().BeNull();
    }

    [Test]
    public async Task When_ValidateEntity_With_NotFoundDevice_Result_Invalid()
    {
        var id = Guid.NewGuid();
        var source = "someSource";
        var expires = DateTimeOffset.Now;
        var apiKey = _apiKeyProcessor.Generate(id, source, expires);

        var result =
            await _apiKeyProcessor.ValidateAsync(apiKey, _dbCotext, _hashHelper);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        result.Item1.Should().Be(ApiKeyValidationResult.DeviceNotFound);
        result.Item2.Should().NotBeNull();
    }

    private async Task AddDeviceWithUser(Guid id, string source, DateTimeOffset expires, string salt, string apiKey)
    {
        var creator = await _dbCotext.Users.FirstAsync(u => u.Username == TestsContext.DefaultUsername);
        await _dbCotext.Devices.AddAsync(new DbDevice
        {
            CreationUTC = DateTimeOffset.UtcNow,
            Creator = creator,
            ExpireUTC = expires,
            Id = id,
            IsActive = true,
            KeyHash = _hashHelper.Hash(apiKey, salt),
            Salt = salt,
            SourceHash = _hashHelper.Hash(source, _hashHelper.DefaultSalt)
        });
        await _dbCotext.SaveChangesAsync();
    }
}