using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Options;
using versioning_manager_api.SystemObjects.Options;
using Aes = System.Security.Cryptography.Aes;

namespace versioning_manager_api.Middle.CryptsProcess;

public interface ICryptHelper
{
    string Encrypt(string text);
    string? Decrypt(string text);
}

public class CryptHelper : ICryptHelper
{
    private readonly byte[] key;
    private readonly byte[] iv;
    private readonly string prefix;

    public CryptHelper(IOptions<ApiKeyOptions> opts)
    {
        key = File.ReadAllBytes(opts.Value.CryptKeyFilePath)[..32];
        iv = File.ReadAllBytes(opts.Value.CryptIVFilePath)[..16];
        prefix = opts.Value.Prefix;
    }
    
    public string Encrypt(string text)
    {
        
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (StreamWriter sw = new(cs))
        {
            sw.Write(text);
        }

        return prefix + Convert.ToBase64String(ms.ToArray());
    }

    public string? Decrypt(string text)
    {
        if (!text.StartsWith(prefix))
        {
            return null;
        }
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream ms = new(Convert.FromBase64String(text[prefix.Length..]));
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