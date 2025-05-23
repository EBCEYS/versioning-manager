using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace versioning_manager_api.Middle.HashProcess;

public interface IHashHelper
{
    /// <summary>
    /// Hashes the password.
    /// </summary>
    /// <param name="pass">The password</param>
    /// <param name="salt">The <see cref="Encoding.ASCII"/> based salt.</param>
    /// <param name="iterations">[optional] The iterations count.</param>
    /// <returns></returns>
    string Hash(string pass, string salt, int iterations = 100000);

    /// <summary>
    /// Generates the salt for hash.
    /// </summary>
    /// <returns>ASCII representation of salt.</returns>
    string GenerateSalt();
    /// <summary>
    /// The default salt.
    /// </summary>
    string DefaultSalt { get; init; }
}

public class HashHelper : IHashHelper
{
    public string Hash(string pass, string salt, int iterations = 100000)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: pass,
            salt: Encoding.ASCII.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: iterations,
            numBytesRequested: 256 / 8));
    }

    public string GenerateSalt()
    {
        return Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(128 / 8));
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