using System.Linq;

namespace GoodFriend.Plugin.Utils
{
    /// <summary> A collection of common utilites </summary>
    public static class Common
    {
        /// <summary>
        ///    Removes all whitespace from the given string
        /// </summary>
        /// <param name="text"> The string to remove whitespace from </param>
        public static string RemoveWhitespace(string text) => new(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
