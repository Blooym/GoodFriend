namespace GoodFriend.Base;

public static class PStrings
{
    /// <summary> The displayed plugin name insode of the Dalamud plugins list and UI titles. </summary>
    public static readonly string pluginName = "Good Friend";

    /// <summary> The repository to attempt to fetch the latest resource files from. Will always attempt to use "main" branch. </summary>
    public static readonly string pluginRepository = "https://github.com/BitsOfAByte/KikoGuide/";

    /// <summary> The path to the plugin's resources folder with trailing slashes </summary>
    public static readonly string resourcePath = $"{Service.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

    /// <summary> The path to the plugin's localization folder with trailing slashes </summary>
    public static readonly string localizationPath = resourcePath + "Localization\\";
}