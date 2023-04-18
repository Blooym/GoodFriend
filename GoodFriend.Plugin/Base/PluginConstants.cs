namespace GoodFriend.Plugin.Base
{
    /// <summary>
    ///     A collection of read-only strings used by the plugin.
    /// </summary>
    public static class PluginConstants
    {
        /// <summary>
        ///     The resources directory relative to the base of the assembly location.
        /// </summary>
        public static readonly string AssemblyResourcesDir = $"{Services.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

        /// <summary>
        ///     The path to the plugin's resources folder with trailing slashes, relative to the plugin assembly location with trailing slashes.
        /// </summary>
        public static readonly string AssemblyLocDir = AssemblyResourcesDir + "Localization\\";
    }
}
