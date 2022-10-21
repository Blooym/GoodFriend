namespace GoodFriend.Utils
{
    using System;
    using System.Linq;

    /// <summary> A collection of common utilites </summary>
    public static class Common
    {
        /// <summary> 
        ///    Removes all whitespace from the given string
        /// </summary>
        /// <param name="text"> The string to remove whitespace from </param>
        public static string RemoveWhitespace(string text) => new String(text.Where((c => !Char.IsWhiteSpace(c))).ToArray());
    }
}