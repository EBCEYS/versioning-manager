using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace versioning_manager_api.Middle.HashProcess;

public interface IHashHelper
{
    /// <summary>
    ///     The default salt.
    /// </summary>
    string DefaultSalt { get; init; }

    /// <summary>
    ///     Hashes the password.
    /// </summary>
    /// <param name="pass">The password</param>
    /// <param name="salt">The <see cref="Encoding.ASCII" /> based salt.</param>
    /// <param name="iterations">[optional] The iterations count.</param>
    /// <returns></returns>
    string Hash(string pass, string salt, int iterations = 100000);

    /// <summary>
    ///     Generates the salt for hash.
    /// </summary>
    /// <returns>ASCII representation of salt.</returns>
    string GenerateSalt();
}

public class HashHelper : IHashHelper
{
    public string Hash(string pass, string salt, int iterations = 100000)
    {
        return Convert.ToHexStringLower(KeyDerivation.Pbkdf2(
            pass,
            Encoding.UTF8.GetBytes(salt),
            KeyDerivationPrf.HMACSHA256,
            iterations,
            256 / 8));
    }

    public string GenerateSalt()
    {
        return Encoding.UTF8.GetString(RandomNumberGenerator.GetBytes(128 / 8)
            .Select(b => b == 0 ? (byte)1 : b)
            .ToArray());
    }

    public string DefaultSalt { get; init; } = "SOME_SOURCE_SALT";
}

public static class PassIServiceCollectionExtensions
{
    public static IServiceCollection AddPasswordHasher(this IServiceCollection sc)
    {
        return sc.AddSingleton<IHashHelper, HashHelper>();
    }
}