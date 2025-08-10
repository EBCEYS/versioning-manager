using FluentAssertions;
using Microsoft.Extensions.Options;
using versioning_manager_api.Middle.CryptsProcess;
using versioning_manager_api.SystemObjects.Options;

namespace versioning_manager_api.UnitTests.Helpers;

public class CryptHelperTests
{
    private const string CryptKeyFilePath1 = "keyFile";
    private const string CryptIvFilePath1 = "IVFile";
    private const string Prefix1 = "abc_";

    private const string CryptKeyFilePath2 = "keyFile2";
    private const string CryptIvFilePath2 = "IVFile2";
    private const string Prefix2 = "abc2_";
    private CryptHelper cryptHelper;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        byte[] keyBytes = new byte[64];
        byte[] ivBytes = new byte[64];
        Random.Shared.NextBytes(keyBytes);
        Random.Shared.NextBytes(ivBytes);
        await Task.WhenAll(File.WriteAllBytesAsync(CryptKeyFilePath1, keyBytes),
            File.WriteAllBytesAsync(CryptIvFilePath1, ivBytes));

        byte[] keyBytes2 = new byte[64];
        byte[] ivBytes2 = new byte[64];
        Random.Shared.NextBytes(keyBytes2);
        Random.Shared.NextBytes(ivBytes2);
        await Task.WhenAll(File.WriteAllBytesAsync(CryptKeyFilePath2, keyBytes2),
            File.WriteAllBytesAsync(CryptIvFilePath2, ivBytes2));
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        RemoveFileIfExists(CryptKeyFilePath1);
        RemoveFileIfExists(CryptIvFilePath1);
        RemoveFileIfExists(CryptKeyFilePath2);
        RemoveFileIfExists(CryptIvFilePath2);
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
            CryptKeyFilePath = CryptKeyFilePath1,
            CryptIVFilePath = CryptIvFilePath1,
            Prefix = Prefix1
        });
        cryptHelper = new CryptHelper(options);
    }

    [TestCaseSource(nameof(GetStrings))]
    public void When_Encrypt_With_SpecifiedParams_Result_CorrectPrefix(string value)
    {
        string encrypted = cryptHelper.Encrypt(value);

        encrypted.StartsWith(Prefix1).Should().BeTrue();
    }

    [TestCaseSource(nameof(GetStrings))]
    public void When_EncryptAndDecrypt_With_SpecifiedParams_Result_Correct(string value)
    {
        string encrypted = cryptHelper.Encrypt(value);
        string? decrypted = cryptHelper.Decrypt(encrypted);

        decrypted.Should().NotBeNull();
        decrypted.Should().Be(value);
    }

    [TestCaseSource(nameof(GetStrings))]
    public void When_EncryptAndDecrypt_With_SpecifiedParamsAndAnotherEncryptor_Result_Correct(string value)
    {
        OptionsWrapper<ApiKeyOptions> options = new(new ApiKeyOptions
        {
            CryptKeyFilePath = CryptKeyFilePath2,
            CryptIVFilePath = CryptIvFilePath2,
            Prefix = Prefix2
        });
        CryptHelper anotherCryptHelper = new(options);

        string encrypted = cryptHelper.Encrypt(value);


        string? decrypted = anotherCryptHelper.Decrypt(encrypted);

        encrypted.StartsWith(Prefix2).Should().BeFalse();
        decrypted.Should().BeNull();
    }

    private static IEnumerable<TestCaseData> GetStrings()
    {
        yield return new TestCaseData(GenerateRandomString());
        yield return new TestCaseData(GenerateRandomString());
        yield return new TestCaseData(GenerateRandomString());
    }

    private static string GenerateRandomString(int length = 8)
    {
        byte[] bytes = new byte[length];
        Random.Shared.NextBytes(bytes);
        return Convert.ToHexStringLower(bytes);
    }
}