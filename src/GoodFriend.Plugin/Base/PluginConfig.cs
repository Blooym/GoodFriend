namespace GoodFriend.Base;

using System;
using Dalamud.Configuration;
using CheapLoc;

enum NotificationType
{
    Toast,
    Chat,
    Popup
}

[Serializable]
sealed class Configuration : IPluginConfiguration
{
    /// <summary> 
    ///     The current configuration version. Incremented whenever breaking changes are made to the configuration.
    /// </summary>
    public int Version { get; set; } = 0;


    /// <summary>
    ///    The API URL to connect to for the events API.
    /// </summary>
    public Uri APIUrl { get; set; } = new Uri("https://aether.bitsofabyte.dev/");
    public void ResetApiUrl() => APIUrl = new Uri("https://aether.bitsofabyte.dev/");


    /// <summary>
    ///     The notification type to use for the plugin.
    /// </summary>
    public NotificationType NotificationType { get; set; } = NotificationType.Chat;


    /// <summary>
    ///     The message to display when a friend logs in.
    /// </summary>
    public string FriendLoggedInMessage { get; set; } = Loc.Localize("UI.Settings.FriendLoggedInMessage", "{0} has logged in.");


    /// <summary>
    ///     The message to display when a friend is logs out.
    /// </summary>
    public string FriendLoggedOutMessage { get; set; } = Loc.Localize("Config.FriendLoggedOutMessage", "{0} has logged out.");


#if DEBUG
    /// <summary>
    ///     The localization export path.
    /// </summary>
    public string localizableOutputDir { get; set; } = Service.PluginInterface.AssemblyLocation?.Directory?.FullName ?? "";
#endif


    /// <summary>
    ///     The "salt" to use when hashing the ContentID.
    ///     Allows users to make their private own notification circles with other people.
    /// </summary>
    public string FriendshipCode { get; set; } = "";


    /// <summary>
    ///     Wehether to hide notifications from players in the same free company.
    /// </summary>
    public bool HideSameFC { get; set; } = false;


    /// <summary>
    ///    Saves the current configuration (and any modifications) to the config file.
    /// </summary>
    internal void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
