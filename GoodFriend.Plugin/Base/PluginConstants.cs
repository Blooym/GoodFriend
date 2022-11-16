using System;
using System.Reflection;

namespace GoodFriend.Base
{
    /// <summary>
    ///     A collection of read-only strings used by the plugin.
    /// </summary>
    public static class PluginConstants
    {
        /// <summary>
        ///     The name of the plugin in the user interface.
        /// </summary>
        public static readonly string PluginName = Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";

        /// <summary>
        ///     The support plugin developer button url.
        /// </summary>
        public static readonly Uri PluginDevSupportUrl = new("https://github.com/sponsors/BitsOfAByte/");

        /// <summary>
        ///     The default API url to use.
        /// </summary>
        public static readonly Uri DefaultAPIUrl = new("https://aether.bitsofabyte.dev/");

        /// <summary>
        ///     The version of the plugin alongside the commit has if available.
        /// </summary>
        public static readonly string Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "???";

        /// <summary>
        ///     The GitHub repository url.
        /// </summary>
        public static readonly Uri RepoUrl = new("https://github.com/BitsOfAByte/GoodFriend/");

        /// <summary>
        ///     The raw GitHub repository url.
        /// </summary>
        public static readonly Uri RawRepoUrl = new("https://raw.githubusercontent.com/BitsOfAByte/GoodFriend/");

        /// <summary>
        ///     The production branch of the repository.
        /// </summary>
        public const string RepoBranch = "main";

        /// <summary>
        ///     The resources directory relative to the base of the repository.
        /// </summary>
        internal const string RepoResourcesDir = "GoodFriend.Plugin/Resources/";

        /// <summary>
        ///     The resources directory relative to the base of the assembly location.
        /// </summary>
        public static readonly string AssemblyResourcesDir = $"{PluginService.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

        /// <summary>
        ///     The path to the plugin's resources folder with trailing slashes, relative to the plugin assembly location with trailing slashes.
        /// </summary>
        public static readonly string AssemblyLocDir = AssemblyResourcesDir + "Localization\\";
    }
}
