using System;
using Dalamud.Utility;

namespace GoodFriend.Plugin.Utility
{
    internal static class ValidatorUtil
    {
        /// <summary>
        ///     Tests if a format string is valid.
        /// </summary>
        /// <param name="message">The message to test.</param>
        /// <param name="numPlaceholders">The number of placeholders expected in the message.</param>
        /// <param name="checkContainAll">Check if the message contains all placeholders.</param>
        /// <returns>Whether or not the message is valid.</returns>
        public static bool TestFormatString(string message, uint numPlaceholders, bool checkContainAll = true)
        {
            try
            {
                message.Format(new object[numPlaceholders]);

                if (checkContainAll && message.Split('{').Length - 1 != numPlaceholders)
                {
                    return false;
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
