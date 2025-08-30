using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Options;
using versioning_manager_api.SystemObjects.Options;
using Aes = System.Security.Cryptography.Aes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace versioning_manager_api.Middle.CryptsProcess;

public interface ICryptHelper
{
    string Encrypt(string text);
    string? Decrypt(string text);
}

public class CryptHelper(IOptions<ApiKeyOptions> opts) : ICryptHelper
{
    private readonly byte[] _iv = File.ReadAllBytes(opts.Value.CryptIVFilePath)[..16];
    private readonly byte[] _key = File.ReadAllBytes(opts.Value.CryptKeyFilePath)[..32];
    private readonly string _prefix = opts.Value.Prefix;

    public string Encrypt(string text)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (StreamWriter sw = new(cs))
        {
            sw.Write(text);
        }

        return _prefix + Convert.ToBase64String(ms.ToArray());
    }

    public string? Decrypt(string text)
    {
        if (!text.StartsWith(_prefix)) return null;
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream ms = new(Convert.FromBase64String(text[_prefix.Length..]));
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        return sr.ReadToEnd();
    }
}

public static class CryptHelperExtensions
{
    public static IServiceCollection AddCryptHelper(this IServiceCollection sc)
    {
        sc.AddDataProtection().UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });
        return sc.AddSingleton<ICryptHelper, CryptHelper>();
    }
}