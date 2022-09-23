namespace GoodFriend.Utils;

using System;
using System.Linq;
using System.Diagnostics;

/// <summary> A collection of common utilites </summary>
public static class Common
{
    /// <summary> Opens the given link in the default application. </summary>
    /// <param name="url"> The link to open. </param>
    public static void OpenLink(string link) => Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });


    /// <summary> Removes all whitespace from the given string </summary>
    /// <param name="text"> The string to remove whitespace from </param>
    public static string RemoveWhitespace(string text) => new String(text.Where((c => !Char.IsWhiteSpace(c))).ToArray());
}
