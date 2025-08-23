namespace versioning_manager_api.IntegrationTests.Mocks;

internal static class FilesCreator
{
    public const string JwtKeyFilePath = "./jwt.key";
    public const string CryptKeyFilePath = "./crypt.key";
    public const string CryptIvFilePath = "./iv.key";
    public const string GitlabKeyFilePath = "./git.key";

    public static async Task Initialize()
    {
        await Task.WhenAll(CreateCryptFiles(), CreateGitlabFile(), CreateJwtFile());
    }

    public static void Cleanup()
    {
        File.Delete(JwtKeyFilePath);
        File.Delete(CryptKeyFilePath);
        File.Delete(CryptIvFilePath);
        File.Delete(GitlabKeyFilePath);
    }

    private static async Task CreateCryptFiles()
    {
        await File.WriteAllBytesAsync(CryptKeyFilePath, GenerateRandomBytes(64));
        await File.WriteAllBytesAsync(CryptIvFilePath, GenerateRandomBytes(64));
    }

    private static async Task CreateGitlabFile()
    {
        var bytes = GenerateRandomBytes(32);
        await File.WriteAllTextAsync(GitlabKeyFilePath, Convert.ToBase64String(bytes));
    }

    private static async Task CreateJwtFile()
    {
        var bytes = GenerateRandomBytes(128);
        await File.WriteAllTextAsync(JwtKeyFilePath, Convert.ToBase64String(bytes));
    }

    private static byte[] GenerateRandomBytes(int length)
    {
        var bytes = new byte[length];
        Random.Shared.NextBytes(bytes);
        return bytes;
    }
}