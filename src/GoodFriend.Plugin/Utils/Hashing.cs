namespace GoodFriend.Utils;

using System;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using GoodFriend.Base;

/// <summary> Salt methods that can be be applied </summary>
public enum SaltMethods : byte
{
    Strict = 0, // Use the players salt (friend code) and the assembly GUID.
    Relaxed = 1, // Only use the players salt (friend code) if set.
}

/// <summary> Hashing implementations and methods </summary>
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
                return PluginService.Configuration.FriendshipCode ?? string.Empty;
        }
    }


    /// <summary> Generates a SHA512 hash from the given string. </summary>
    /// <param name="input"> The string to hash. </param>
    /// <returns> The hashed string. </returns>
    public static string HashSHA512(string input)
    {
        var salt = CreateSalt(PluginService.Configuration.SaltMethod);
        var bytes = Encoding.UTF8.GetBytes(input + salt);
        return Convert.ToBase64String(SHA512.Create().ComputeHash(bytes));
    }
}
