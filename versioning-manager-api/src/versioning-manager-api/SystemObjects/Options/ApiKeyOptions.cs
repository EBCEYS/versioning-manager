#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace versioning_manager_api.SystemObjects.Options;

public class ApiKeyOptions
{
    public required string CryptKeyFilePath { get; init; }

    // ReSharper disable once InconsistentNaming
    public required string CryptIVFilePath { get; init; }
    public required string Prefix { get; init; }
}