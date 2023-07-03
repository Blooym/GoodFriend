using System;
using System.Security.Cryptography;
using System.Text;
using GoodFriend.Plugin.Base;

#if RELEASE
using System.Reflection;
#endif

namespace GoodFriend.Plugin.Utility
{
    internal static class CryptoUtil
    {
        /// <summary>
        ///     Gets signature bytes for the current assembly.
        /// </summary>
        /// <remarks>
        ///    This is used as a salt for hashing values in release builds as it allows only users on the same build to
        ///    read the hashed values unless this value is reverse engineered - although it will still change on every
        ///    build.
        /// </remarks>
        /// <returns>The signature bytes in release builds, otherwise an empty array to for development builds.</returns>
        public static byte[] GetSignatureBytes() =>
#if RELEASE
            Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray();
#else
            Array.Empty<byte>(); // Easier debugging across dev builds.
#endif

        /// <summary>
        ///     Generates a random salt of the given length.
        /// </summary>
        /// <param name="length">The length of the salt to generate.</param>
        /// <returns>The generated salt.</returns>
        public static string GenerateSalt(uint length = 32)
        {
            using var seed = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            Logger.Information(GetSignatureBytes().Length.ToString());
            seed.GetBytes(bytes);
            seed.GetBytes(GetSignatureBytes());
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        ///     Hashes the given value and returns it as a hex string.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hashed value.</returns>
        public static string Hash(object value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{value}")));

        /// <summary>
        ///     Hashes the given value and returns it as a hex string.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>The hashed value.</returns>
        public static string HashWithSalt(object value, string salt) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{value}{salt}")));
    }
}
