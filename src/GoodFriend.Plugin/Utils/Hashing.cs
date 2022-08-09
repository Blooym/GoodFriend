namespace GoodFriend.Utils;

using System;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using GoodFriend.Base;

enum SaltMethods : byte
{
    Relaxed = 0, // Only use the players salt (friend code) if set.
    Strict = 1, // Use the players salt (friend code) and the assembly GUID.
}

public static class Hashing
{

    /// <summary> Generates a salt using a given method and length. </summary>
    /// <param name="method"> The method to use for generating the salt. </param>
    /// <returns> The generated salt. </returns>
    private static string? CreateSalt(SaltMethods method)
    {
        switch (method)
        {
            case SaltMethods.Relaxed:
                return PluginService.Configuration.FriendshipCode ?? string.Empty;
            case SaltMethods.Strict:
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString() + PluginService.Configuration.FriendshipCode;
            default:
                throw new Exception("Hashing: Failed to generate salt, invalid salt method.");
        }
    }


    /// <summary> Generates a SHA512 hash from the given string. </summary>
    public static string HashSHA512(string input)
    {
        var salt = CreateSalt(SaltMethods.Strict);
        var bytes = Encoding.UTF8.GetBytes(input + salt);

        return Convert.ToBase64String(SHA512.Create().ComputeHash(bytes));
    }
}


