using System;
// using System.Globalization;
using System.Linq;
using GoodFriend.Base;

namespace GoodFriend.Utils
{

    /// <summary> A collection of common utilites </summary>
    public static class Common
    {
        /// <summary>
        ///    Removes all whitespace from the given string
        /// </summary>
        /// <param name="text"> The string to remove whitespace from </param>
        public static string RemoveWhitespace(string text) => new(text.Where(c => !char.IsWhiteSpace(c)).ToArray());


        /// <summary>
        ///     Checks to see if the plugin came from an official source
        /// </summary>
        // public static bool IsOfficialSource =>
        // PluginService.PluginInterface.SourceRepository.StartsWith("https://kamori.goats.dev/Plugin/PluginMaster", true, CultureInfo.InvariantCulture) ||
        // PluginService.PluginInterface.SourceRepository.StartsWith(PluginConstants.RawRepoUrl.ToString(), true, CultureInfo.InvariantCulture);

        /// <summary>
        ///     Checks to see if the plugin is a dev build
        /// </summary>
        public static bool IsDevBuild => PluginService.PluginInterface.IsDev;

        /// <summary>
        ///     Returns the operating system of the executing system.
        /// </summary>
        public static string GetOperatingSystem()
        {
            var wineOnLinux = Environment.GetEnvironmentVariable("XL_WINEONLINUX");
            if (wineOnLinux == "1" || wineOnLinux?.ToLower() == "true")
            {
                return "Linux (Wine)";
            }

            var wineOnMac = Environment.GetEnvironmentVariable("XL_WINEONMAC");
            if (wineOnMac == "1" || wineOnMac?.ToLower() == "true")
            {
                return "OSX (Wine)";
            }

            var winePrefix = Environment.GetEnvironmentVariable("WINEPREFIX");
            if (!string.IsNullOrEmpty(winePrefix))
            {
                return "Unknown (Wine)";
            }

#pragma warning disable IDE0072 // Populate switch expression
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "Windows",
                PlatformID.Unix => "Linux",
                PlatformID.MacOSX => "OSX",
                _ => "Unknown",
            };
#pragma warning restore IDE0072 // Populate switch expression
        }
    }
}
