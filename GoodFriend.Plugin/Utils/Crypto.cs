using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace GoodFriend.Plugin.Utils
{
    /// <summary> Crypto implementations and methods </summary>
    internal static class CryptoUtil
    {
        /// <summary>
        ///     Generates a salt using a given method and length.
        /// </summary>
        /// <param name="method"> The <see cref="SaltMethods"/> to use. </param>
        /// <returns> The generated salt. </returns>
        private static string? CreateSalt() => Common.RemoveWhitespace(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString());

        /// <summary>
        ///     Creates a random string of a given length.
        /// </summary>
        /// <param name="length"> The length of the string to generate. </param>
        /// <returns> The generated string. </returns>
        public static string GenerateRandom(int length)
        {
            var random = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            random.GetBytes(bytes);
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        ///     Generates a SHA512 hash from the given string.
        /// </summary>
        /// <param name="input"> The string to hash. </param>
        /// <param name="addedSalt"> The extra salt to add to the hash. </param>
        /// <returns> The hashed string. </returns>
        internal static string HashSHA512(string input, string addedSalt)
        {
            var salt = CreateSalt();
            var bytes = Encoding.UTF8.GetBytes(input + salt + addedSalt);
            return Convert.ToHexString(SHA512.HashData(bytes));
        }
    }
}
