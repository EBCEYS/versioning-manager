namespace versioning_manager_api.SystemObjects.Options;

public class ApiKeyOptions
{
    public required string CryptKeyFilePath { get; init; }
    public required string CryptIVFilePath { get; init; }
    public required string Prefix { get; init; }
}