namespace GoodFriend.Base;

using System;
using Dalamud.Configuration;
using CheapLoc;
using GoodFriend.Utils;

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
    ///  The current configuration version. Incremented whenever breaking changes are made to the configuration.
    /// </summary>
    public int Version { get; set; } = 0;


    ///////////////////////
    // Generic  Settings //
    ///////////////////////

    /// <summary>
    ///  The notification type to use for friend notifications.
    /// </summary>
    public NotificationType NotificationType { get; set; } = NotificationType.Chat;


    /// <summary>
    /// The message to display when a friend logs in.
    /// </summary>
    public string FriendLoggedInMessage { get; set; } = Loc.Localize("UI.Settings.FriendLoggedInMessage", "{0} has logged in.");


    /// <summary>
    ///  The message to display when a friend is logs out.
    /// </summary>
    public string FriendLoggedOutMessage { get; set; } = Loc.Localize("Config.FriendLoggedOutMessage", "{0} has logged out.");


    /// <summary>
    ///  Whether or not to hide notifications for the same free company.
    /// </summary>
    public bool HideSameFC { get; set; } = true;


    ///////////////////////
    // Security Settings //
    ///////////////////////

    /// <summary>
    /// The "salt" to use when hashing the ContentIDs of players.
    /// </summary>
    public string FriendshipCode { get; set; } = "";


    /// <summary>
    /// The salt method the plugin should use when hashing.
    /// </summary>
    public SaltMethods SaltMethod { get; set; } = SaltMethods.Strict;


    /// <summary>
    ///  The API to use for notification sending and receiving.
    /// </summary>
    public Uri APIUrl { get; set; } = PStrings.apiURL;
    public void ResetApiUrl() { this.APIUrl = PStrings.apiURL; this.Save(); }


    ///////////////////////
    //  Debug  Settings  //
    ///////////////////////

#if DEBUG
    /// <summary>
    /// The localization export path.
    /// </summary>
    public string localizableOutputDir { get; set; } = PluginService.PluginInterface.AssemblyLocation?.Directory?.FullName ?? "";
#endif


    /// <summary>
    ///  Saves the current configuration (and any modifications) to the config file.
    /// </summary>
    internal void Save()
    {
        PluginService.PluginInterface.SavePluginConfig(this);
    }
}
