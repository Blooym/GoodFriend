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

        /// <summary>
        ///     Returns the operating system of the executing system.
        /// </summary>
        public static string GetOperatingSystem()
        {
            var wineOnLinux = Environment.GetEnvironmentVariable("XL_WINEONLINUX");
            if (wineOnLinux == "1" || wineOnLinux?.ToLower() == "true")
                return "Linux (Wine)";

            var wineOnMac = Environment.GetEnvironmentVariable("XL_WINEONMAC");
            if (wineOnMac == "1" || wineOnMac?.ToLower() == "true")
                return "OSX (Wine)";

            var winePrefix = Environment.GetEnvironmentVariable("WINEPREFIX");
            if (!string.IsNullOrEmpty(winePrefix))
                return "Unknown (Wine)";

            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "Windows",
                PlatformID.Unix => "Linux",
                PlatformID.MacOSX => "OSX",
                _ => "Unknown",
            };
        }
    }
}