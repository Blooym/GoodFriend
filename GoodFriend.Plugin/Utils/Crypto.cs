using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GoodFriend.Base;

namespace GoodFriend.Utils
{
    /// <summary>
    ///     Salt methods that can be applied.
    /// </summary>
    public enum SaltMethods : byte
    {
        /// <summary>
        /// Use the players salt (friend code) and the assembly manifest.
        /// </summary>
        Strict = 0,

        /// <summary>
        /// Only use the players salt (friend code) if set.
        /// </summary>
        Relaxed = 1,
    }

    /// <summary> Crypto implementations and methods </summary>
    internal static class CryptoUtil
    {
        /// <summary>
        ///     Generates a salt using a given method and length.
        /// </summary>
        /// <param name="method"> The <see cref="SaltMethods"/> to use. </param>
        /// <returns> The generated salt. </returns>
        private static string? CreateSalt(SaltMethods method) => method switch
        {
            SaltMethods.Relaxed => PluginService.Configuration.FriendshipCode ?? string.Empty,
            SaltMethods.Strict => Common.RemoveWhitespace(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString() + PluginService.Configuration.FriendshipCode),
            _ => PluginService.Configuration.FriendshipCode ?? string.Empty,
        };

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
            var salt = CreateSalt(PluginService.Configuration.SaltMethod);
            var bytes = Encoding.UTF8.GetBytes(input + salt + addedSalt);
            return Convert.ToHexString(SHA512.HashData(bytes));
        }
    }
}
