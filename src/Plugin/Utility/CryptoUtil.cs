using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace GoodFriend.Plugin.Utility;

internal static class CryptoUtil
{
    /// <summary>
    ///     Gets the module version identifier of the current assembly.
    /// </summary>
    /// <remarks>
    ///    This is used as a salt for hashing values as it allows only users on the same build to
    ///    read the hashed values unless this value is extracted manually.
    /// </remarks>
    private static Guid GetModuleVersionId() => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId;

    /// <summary>
    ///     Generates a random salt of the given length.
    /// </summary>
    /// <param name="length">The length of the salt to generate.</param>
    public static string GenerateSalt(uint length = 32)
    {
        using var seed = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        seed.GetBytes(bytes);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    ///     Hashes the given value and returns it as a hex string.
    /// </summary>
    /// <param name="value">The value to hash.</param>
    /// <param name="salt">The salt to use.</param>
    public static string HashValue(object value, string salt)
    {
        var toHash = $"{value}{DateTime.UtcNow:yyyyMMddHH}{salt}{GetModuleVersionId()}";
        var outcome = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(toHash)));
        return outcome;
    }
}
