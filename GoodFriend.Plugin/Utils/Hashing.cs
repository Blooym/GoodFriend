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

    /// <summary> Hashing implementations and methods </summary>
    internal static class Hashing
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
        ///     Generates a SHA512 hash from the given string.
        /// </summary>
        /// <param name="input"> The string to hash. </param>
        /// <returns> The hashed string. </returns>
        internal static string HashSHA512(string input)
        {
            var salt = CreateSalt(PluginService.Configuration.SaltMethod);
            var bytes = Encoding.UTF8.GetBytes(input + salt);
            return Convert.ToBase64String(SHA512.Create().ComputeHash(bytes)).Replace("_", "q").Replace("+", "v");
        }
    }
}
