using FluentAssertions;
using Microsoft.Extensions.Options;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Middle.ApiKeyProcess;
using versioning_manager_api.Middle.CryptsProcess;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.SystemObjects.Options;
using versioning_manager_api.UnitTests.Mocks;

namespace versioning_manager_api.UnitTests.Helpers;

public class ApiKeyProcessorTests
{
    private const string CryptKeyFilePath = "keyFile";
    private const string CryptIvFilePath = "IVFile";
    private const string Prefix = "abc_";

    private ApiKeyProcessor apiKeyProcessor;
    private VmDatabaseDbContextMockHelper dbContextHelper;
    private HashHelper hashHelper;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var keyBytes = new byte[64];
        var ivBytes = new byte[64];
        Random.Shared.NextBytes(keyBytes);
        Random.Shared.NextBytes(ivBytes);
        await Task.WhenAll(File.WriteAllBytesAsync(CryptKeyFilePath, keyBytes),
            File.WriteAllBytesAsync(CryptIvFilePath, ivBytes));
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        RemoveFileIfExists(CryptKeyFilePath);
        RemoveFileIfExists(CryptIvFilePath);
        return;

        void RemoveFileIfExists(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [SetUp]
    public void Initialize()
    {
        OptionsWrapper<ApiKeyOptions> options = new(new ApiKeyOptions
        {
            CryptKeyFilePath = CryptKeyFilePath,
            CryptIVFilePath = CryptIvFilePath,
            Prefix = Prefix
        });
        ICryptHelper cryptHelper = new CryptHelper(options);
        apiKeyProcessor = new ApiKeyProcessor(cryptHelper);

        hashHelper = new HashHelper();

        dbContextHelper = new VmDatabaseDbContextMockHelper();
    }

    [TestCaseSource(nameof(GetDataForTests))]
    public void When_SomeEntityKeyed_With_SpecifiedParams_Result_Success(Guid id, string source, DateTimeOffset expires)
    {
        var apiKey = apiKeyProcessor.Generate(id, source, expires);

        var entity = apiKeyProcessor.Decrypt(apiKey);

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
        var apiKey = apiKeyProcessor.Generate(id, source, expires);

        var salt = hashHelper.GenerateSalt();

        AddDeviceWithUser(id, source, expires, salt, apiKey);

        var result =
            await apiKeyProcessor.ValidateAsync(apiKey, dbContextHelper.Initialize(), hashHelper);

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
        var apiKey = apiKeyProcessor.Generate(id, source, expires);

        var salt = hashHelper.GenerateSalt();

        AddDeviceWithUser(id, source, expires, salt, apiKey);

        var result =
            await apiKeyProcessor.ValidateAsync(apiKey, dbContextHelper.Initialize(), hashHelper);

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
        var apiKey = apiKeyProcessor.Generate(id, source, expires);

        var salt = hashHelper.GenerateSalt();

        AddDeviceWithUser(id, "AnotherSalt", expires, salt, apiKey);

        var result =
            await apiKeyProcessor.ValidateAsync(apiKey, dbContextHelper.Initialize(), hashHelper);

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
        var apiKey = apiKeyProcessor.Generate(id, source, expires) + "some string";

        var salt = hashHelper.GenerateSalt();

        AddDeviceWithUser(id, source, expires, salt, apiKey);

        var result =
            await apiKeyProcessor.ValidateAsync(apiKey, dbContextHelper.Initialize(), hashHelper);

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
        var apiKey = apiKeyProcessor.Generate(id, source, expires);

        var result =
            await apiKeyProcessor.ValidateAsync(apiKey, dbContextHelper.Initialize(), hashHelper);

        apiKey.StartsWith(Prefix).Should().BeTrue();
        result.Item1.Should().Be(ApiKeyValidationResult.DeviceNotFound);
        result.Item2.Should().NotBeNull();
    }

    private void AddDeviceWithUser(Guid id, string source, DateTimeOffset expires, string salt, string apiKey)
    {
        dbContextHelper.Devices.Add(new DbDevice
        {
            CreationUTC = DateTimeOffset.UtcNow,
            Creator = new DbUser
            {
                IsActive = true,
                Salt = salt,
                Password = hashHelper.Hash("password", salt),
                Username = "Vitaliy",
                CreationUtc = DateTimeOffset.UtcNow
            },
            CreatorId = 1,
            ExpireUTC = expires,
            Id = id,
            IsActive = true,
            KeyHash = hashHelper.Hash(apiKey, salt),
            Salt = salt,
            SourceHash = hashHelper.Hash(source, hashHelper.DefaultSalt)
        });
    }
}