namespace GoodFriend.Base;

using System;
using Dalamud.Configuration;
using GoodFriend.Utils;

public enum NotificationType
{
    Toast,
    Chat,
    Popup
}

[Serializable]
sealed public class Configuration : IPluginConfiguration
{
    /// <summary>
    ///     The current configuration version, incremented on breaking changes.
    /// </summary>
    public int Version { get; set; } = 0;


    ///////////////////////
    // Generic  Settings //
    ///////////////////////

    /// <summary>
    ///     The notification type to use for friend notifications.
    /// </summary>
    public NotificationType NotificationType { get; set; } = NotificationType.Chat;

    /// <summary>
    ///     The message to display when a friend logs in.
    /// </summary>
    public string FriendLoggedInMessage { get; set; } = "{0} has logged in.";

    /// <summary>
    ///     The message to display when a friend logs out.
    /// </summary>
    public string FriendLoggedOutMessage { get; set; } = "{0} has logged out.";

    /// <summary> 
    ///     Whether or not to hide notifications for the same free company.
    /// </summary>
    public bool HideSameFC { get; set; } = true;

    /// <summary>
    ///     Whether or not to show API events as notifications.
    /// </summary>
    public bool ShowAPIEvents { get; set; } = false;


    ///////////////////////
    // Security Settings //
    ///////////////////////

    /// <summary> 
    ///     The "salt" to use when hashing the ContentIDs of players.
    /// </summary>
    public string FriendshipCode { get; set; } = "";

    /// <summary>
    ///     The salt method to use when hashing.
    /// </summary>
    public SaltMethods SaltMethod { get; set; } = SaltMethods.Strict;

    /// <summary>
    ///     The API to communicate with.
    /// </summary>
    public Uri APIUrl { get; set; } = PStrings.defaultAPIUrl;


    ///////////////////////
    //      Methods      //
    ///////////////////////

    /// <summary>
    ///     Saves the current configuration (and any modifications) to the config file.
    /// </summary>
    public void Save() => PluginService.PluginInterface.SavePluginConfig(this);

    /// <summary>
    ///     Resets the API URL to the default and saves.
    /// </summary>
    public void ResetApiUrl() { APIUrl = PStrings.defaultAPIUrl; Save(); }
}
