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
        public static readonly string pluginName = Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";

        /// <summary>
        ///     The support plugin developer button url.
        /// </summary>
        public static readonly Uri pluginDevSupportUrl = new("https://github.com/sponsors/BitsOfAByte/");

        /// <summary>
        ///     The default API url to use.
        /// </summary>
        public static readonly Uri defaultAPIUrl = new("https://aether.bitsofabyte.dev/");

        /// <summary>
        ///     The version of the plugin alongside the commit has if available.
        /// </summary>
        public static readonly string version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "???";

        /// <summary>
        ///     The GitHub repository url.
        /// </summary>
        public static readonly Uri repoUrl = new("https://github.com/BitsOfAByte/GoodFriend/");

        /// <summary>
        ///     The raw GitHub repository url.
        /// </summary>
        public static readonly Uri rawRepoUrl = new("https://raw.githubusercontent.com/BitsOfAByte/GoodFriend/");

        /// <summary>
        ///     The production branch of the repository.
        /// </summary>
        public static readonly string repoBranch = "main";

        /// <summary>
        ///     The resources directory relative to the base of the repository.
        /// </summary>
        internal static readonly string repoResourcesDir = "src/GoodFriend.Plugin/Resources/";

        /// <summary>
        ///     The resources directory relative to the base of the the assembly location.
        /// </summary>
        public static readonly string assemblyResourcesDir = $"{PluginService.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

        /// <summary>
        ///     The path to the plugin's resources folder with trailing slashes, relative to the plugin assembly location with trailing slashes.
        /// </summary>
        public static readonly string assemblyLocDir = assemblyResourcesDir + "Localization\\";
    }
}