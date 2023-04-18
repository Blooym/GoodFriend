using System;
using System.Security.Cryptography;
using System.Text;

namespace GoodFriend.Client.Security
{
    internal static class Crypto
    {
        /// <summary>
        ///     Generates a cryptographically random hex string.
        /// </summary>
        /// <param name="length">The length of the hex string.</param>
        /// <returns>A randomly generated hex string of the given length.</returns>
        internal static string GenerateCryptoRandom(int length)
        {
            var random = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            random.GetBytes(bytes);
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        ///     Generates a cryptographically random SHA512 hash with the given salt applied.
        /// </summary>
        /// <param name="input">The plaintext to hash.</param>
        /// <param name="salt">The salt to use alongside the hash.</param>
        /// <returns>A salted SHA512 hash.</returns>
        internal static string HashSHA512(string input, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(input + salt);
            return Convert.ToHexString(SHA512.HashData(bytes));
        }
    }
}