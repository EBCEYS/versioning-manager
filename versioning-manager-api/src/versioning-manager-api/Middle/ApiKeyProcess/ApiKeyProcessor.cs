using Microsoft.EntityFrameworkCore;
using versioning_manager_api.DevDatabase;
using versioning_manager_api.Middle.CryptsProcess;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Middle.ApiKeyProcess;

/// <summary>
/// Tha apikey processor interface.
/// </summary>
public interface IApiKeyProcessor
{
    /// <summary>
    /// Generates the apikey.
    /// </summary>
    /// <param name="id">The device id.</param>
    /// <param name="source">The source.</param>
    /// <param name="expires">The apikey expires datetime UTC.</param>
    /// <returns></returns>
    string Generate(Guid id, string source, DateTimeOffset expires);
    /// <summary>
    /// Decrypts apikey to <see cref="ApiKeyEntity"/>.
    /// </summary>
    /// <param name="text">The encrypted apikey.</param>
    /// <returns>An instance of <see cref="ApiKeyEntity"/> if decrypted successfully; otherwise <c>null</c>.</returns>
    ApiKeyEntity? Decrypt(string? text);

    /// <summary>
    /// Validates apikey.
    /// </summary>
    /// <param name="encryptedKey">The encrypted key.</param>
    /// <param name="db">The database context.</param>
    /// <param name="hasher">The hasher.</param>
    /// <returns><see cref="ApiKeyValidationResult.Valid"/> if validated successfully; otherwise another <see cref="ApiKeyValidationResult"/>.</returns>
    public Task<(ApiKeyValidationResult, ApiKeyEntity?)> ValidateAsync(string encryptedKey, VmDatabaseContext db, IHashHelper hasher);
}
/// <summary>
/// The apikey processor.
/// </summary>
/// <param name="crypt">The crypt helper instance.</param>
public class ApiKeyProcessor(ICryptHelper crypt) : IApiKeyProcessor
{
    /// <inheritdoc />
    public string Generate(Guid id, string source, DateTimeOffset expires)
    {
        ApiKeyEntity key = new()
        {
            DeviceId = id,
            Source = source,
            ExpiresUtc = expires
        };

        return crypt.Encrypt(key.ToJson());
    }

    /// <inheritdoc />
    public ApiKeyEntity? Decrypt(string? text)
    {
        if (text == null)
        {
            return null;
        }
        try
        {
            string? json = crypt.Decrypt(text);
            return ApiKeyEntity.FromJson(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<(ApiKeyValidationResult, ApiKeyEntity?)> ValidateAsync(string encryptedKey, VmDatabaseContext db, IHashHelper hasher)
    {
        ApiKeyEntity? key = Decrypt(encryptedKey);
        if (key == null)
        {
            return (ApiKeyValidationResult.IncorrectKey, key);
        }
        DbDevice? device = await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == key.DeviceId && d.IsActive);
        if (device == null)
        {
            return (ApiKeyValidationResult.DeviceNotFound, key);
        }

        string keyHash = hasher.Hash(encryptedKey, device.Salt);
        if (keyHash != device.KeyHash)
        {
            return (ApiKeyValidationResult.IncorrectKey, key);
        }

        string sourceHash = hasher.Hash(key.Source, hasher.DefaultSalt);
        if (sourceHash != device.SourceHash)
        {
            return (ApiKeyValidationResult.IncorrectSource, key);
        }

        return DateTimeOffset.UtcNow <= key.ExpiresUtc ? (ApiKeyValidationResult.Valid, key) : (ApiKeyValidationResult.Expired, key);
    }
}
/// <summary>
/// The apikey validation result.
/// </summary>
public enum ApiKeyValidationResult
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown,
    /// <summary>
    /// Device not found.
    /// </summary>
    DeviceNotFound,
    /// <summary>
    /// The incorrect key.
    /// </summary>
    IncorrectKey,
    /// <summary>
    /// The incorrect source.
    /// </summary>
    IncorrectSource,
    /// <summary>
    /// Key expired.
    /// </summary>
    Expired,
    /// <summary>
    /// Key is valid.
    /// </summary>
    Valid
}
/// <summary>
/// The apikey processor extensions.
/// </summary>
public static class ApiKeyProcessorExtensions
{
    /// <summary>
    /// Adds <see cref="IApiKeyProcessor"/> to <paramref name="sc"/> as singleton.
    /// </summary>
    /// <param name="sc">The service collection.</param>
    /// <returns>An instance of <paramref name="sc"/>.</returns>
    public static IServiceCollection AddApiKeyProcessor(this IServiceCollection sc)
    {
        return sc.AddSingleton<IApiKeyProcessor, ApiKeyProcessor>();
    }
}