namespace GoodFriend.Base;

using System;

public static class PStrings
{
    public static readonly string pluginName = "Good Friend";

    public static readonly string pluginRepository = "https://github.com/BitsOfAByte/GoodFriend/";

    public static readonly string resourcePath = $"{PluginService.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

    public static readonly string localizationPath = resourcePath + "Localization\\";

    public static readonly string supportButtonUrl = "https://github.com/sponsors/BitsOfAByte";

    public static readonly Uri apiURL = new Uri("https://aether.bitsofabyte.dev/");
}