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

    public static readonly Uri apiURL = new Uri("https://aether.bitsofabyte.dev/");
}


/// <summary> A collection of strings used for user facing messages and are translated by the plugin. </summary>
public static class TStrings
{
    public static string SupportText() => String.Format(Loc.Localize("SupportText", "Support the developer of {0}"), PStrings.pluginName);
    public static string SettingsTitle() => String.Format(Loc.Localize("Settings.Title", "{0} - Settings"), PStrings.pluginName);
    public static string SettingsAPIConnected(bool status) => String.Format(Loc.Localize("Settings.APIConnected", "API Connected: {0}"), status);
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
    public static string SettingsSaltModeStrict() => Loc.Localize("Settings.SaltMode.Strict", "Strict");
    public static string SettingsSaltModeRelaxed() => Loc.Localize("Settings.SaltMode.Relaxed", "Relaxed");
    public static string SettingsSaltModeTooltip() => Loc.Localize("Settings.SaltMode.Tooltip", "Strict: Validation is done using both Friend Code & Plugin Assembly\nRelaxed: Validation is done using Friend Code\n\nKeep this on strict if you are not having any issues");
    public static string SettingsSaltModeWarning() => Loc.Localize("Settings.SaltMode.Warning", "Warning: Relaxed mode is not recommended unless you have an explicit reason for using it.\n\nYou will not recieve notifications from Strict mode users");
    public static string SettingsAPIURL() => Loc.Localize("Settings.APIURL", "API URL");
    public static string SettingsAPIURLTooltip() => Loc.Localize("Settings.APIURL.Tooltip", "The API is used to send and recieve notifications from friends using the same API.\n\nYou should not change this unless you know what you are doing.");
    public static string SettingsAPINotHttps() => Loc.Localize("Settings.APIURL.NotHttps", "Warning: You are not using HTTPs for the API! This is VERY insecure and could lead to major security problems.");
    public static string SettingsAPIReconnect() => Loc.Localize("Settings.API.Reconnect", "Reconnect to the API");
}