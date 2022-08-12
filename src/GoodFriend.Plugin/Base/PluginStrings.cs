namespace GoodFriend.Base;

using System;
using CheapLoc;


/// <summary> A collection of read-only strings used by the plugin. </summary>
public static class PStrings
{
    public static readonly string pluginName = "Good Friend";

    public static readonly string pluginRepository = "https://github.com/BitsOfAByte/GoodFriend/";

    public static readonly string resourcePath = $"{PluginService.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

    public static readonly string localizationPath = resourcePath + "Localization\\";

    public static readonly string supportButtonUrl = "https://github.com/sponsors/BitsOfAByte";

    public static readonly Uri defaultAPIUrl = new Uri("https://aether.bitsofabyte.dev/");
}


/// <summary> A collection of strings used for user facing messages and are translated by the plugin. </summary>
public static class TStrings
{
    public static string SettingsTabSettings() => Loc.Localize("Settings.Tab.Settings", "Settings");
    public static string SettingsTabConnection() => Loc.Localize("Settings.Tab.Connection", "Connection");
    public static string SettingsTabDebug() => Loc.Localize("Settings.Tab.Debug", "Debug");
    public static string SettingsSupportText() => Loc.Localize("Settings.Support", "Donate / Support");
    public static string SettingsAPIConnected() => Loc.Localize("Settings.Connection.APIConnected", "You are connected to the API and recieving events");
    public static string SettingsAPIConnectedDesc() => Loc.Localize("Settings.Connections.APIConnected.Desc", "You will see friend events in the log below when you recieve them.");
    public static string SettingsAPIDisconnected() => Loc.Localize("Settings.Connection.APIDisconnected", "You are not connected to the API are not recieving events.");
    public static string SettingsAPIDisconnectedDesc() => Loc.Localize("Settings.Connection.APIDisconnectedDesc", "You should be automatically reconnected when the API becomes available, if you wish to reconnect manually you can try use the button below. For more information about why you're not connected, check /xllog");
    public static string SettingsAPITryReconnect() => Loc.Localize("Settings.Connection.TryReconnect", "Attempt to reconnect");
    public static string SettingsAPINotLoggedIn() => Loc.Localize("Settings.Connection.NotLoggedIn", "You must log in to a character to connect to the API");
    public static string SettingsAPILogTitle() => Loc.Localize("Settings.Connection.Log", "Event Log");
    public static string SettingsAPILogTime() => Loc.Localize("Settings.Connection.Log.Time", "Time");
    public static string SettingsAPILogPlayer() => Loc.Localize("Settings.Connection.Log.Player", "Player");
    public static string SettingsAPILogEvent() => Loc.Localize("Settings.Connection.Log.Event", "Event");
    public static string SettingsHideSameFC() => Loc.Localize("Settings.HideSameFC", "Hide FC Members");
    public static string SettingsHideSameFCEnabled() => Loc.Localize("Settings.HideSameFC.Enabled", "Enabled");
    public static string SettingsHideSameFCDisabled() => Loc.Localize("Settings.HideSameFC.Disabled", "Disabled");
    public static string SettingsHideSameTCTooltip() => Loc.Localize("Settings.HideSameFC.Tooltip", "When enabled, you will not be notified when a friend in your free company logs in or out.");
    public static string SettingsNotificationType() => Loc.Localize("Settings.NotificationType", "Notification Type");
    public static string SettingsNotificationTypeTooltip() => Loc.Localize("Settings.NotificationType.Tooltip", "The place to display notifications when a friend logs in or logs out.");
    public static string SettingsLoginMessage() => Loc.Localize("Settings.LoginMessage", "Login Message");
    public static string SettingsLoginMessageTooltip() => Loc.Localize("Settings.LoginMessage.Tooltip", "The message to show when a friend logs in.\n\n{0}: Name of the friend.");
    public static string SettingsLogoutMessage() => Loc.Localize("Settings.LogoutMessage", "Logout Message");
    public static string SettingsLogoutMessageTooltip() => Loc.Localize("Settings.LogoutMessage.Tooltip", "The message to show when a friend logs out.\n\n{0}: Name of the friend.");
    public static string SettingsFriendshipCode() => Loc.Localize("Settings.FriendshipCode", "Friendship Code");
    public static string SettingsFriendshipCodeHint() => Loc.Localize("Settings.FriendshipCode.Hint", "Leave empty to send notifications to all friends");
    public static string SettingsFriendshipCodeTooltip() => Loc.Localize("Settings.FriendshipCode.Tooltip", "Your friend code determines which friends you send and recieve notifications with.");
    public static string SettingsShowAdvanced() => Loc.Localize("Settings.ShowAdvanced", "Show Advanced Settings");
    public static string SettingsSaltMode() => Loc.Localize("Settings.SaltMode", "Salt Mode");
    public static string SettingsSaltModeTooltip() => Loc.Localize("Settings.SaltMode.Tooltip", "Strict: Validation is done using both Friend Code & Plugin Assembly\nRelaxed: Validation is done using Friend Code\n\nKeep this on strict if you are not having any issues");
    public static string SettingsAPIURL() => Loc.Localize("Settings.APIURL", "API URL");
    public static string SettingsAPIURLTooltip() => Loc.Localize("Settings.APIURL.Tooltip", "The API used to handle events.");
}
