namespace GoodFriend.Utils;

using System;
using System.Text;
using System.Security.Cryptography;
using GoodFriend.Base;

class Hashing
{
    public static string Hash(string input) => Convert.ToBase64String(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes($"{input}{Service.Configuration.EventSecret}")));
}