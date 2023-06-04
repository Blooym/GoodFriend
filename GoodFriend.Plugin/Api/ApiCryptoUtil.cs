using System;
using System.Security.Cryptography;
using System.Text;

namespace GoodFriend.Plugin.Api
{
    internal static class ApiCryptoUtil
    {
        /// <summary>
        ///     The recommended length for a salt value.
        /// </summary>
        private const uint RECOMMENDED_SALT_LENGTH = 32;

        /// <summary>
        ///     Generates a random salt of the given length.
        /// </summary>
        /// <param name="length">The length of the salt to generate.</param>
        /// <returns>The generated salt.</returns>
        public static string GenerateSalt(uint length = RECOMMENDED_SALT_LENGTH)
        {
            var random = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            random.GetBytes(bytes);
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        ///     Hashes the given value and returns it as a hex string.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>The hashed value.</returns>
        public static string HashValue(object value, string salt) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{value}{salt}")));
    }
}
