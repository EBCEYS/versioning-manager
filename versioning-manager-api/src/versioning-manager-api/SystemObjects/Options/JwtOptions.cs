#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace versioning_manager_api.SystemObjects.Options;

public class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SecretFilePath { get; init; }
    public TimeSpan? TokenTimeToLive { get; init; }
}