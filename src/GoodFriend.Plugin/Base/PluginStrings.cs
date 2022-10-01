namespace GoodFriend.Base;

using System;
using CheapLoc;


/// <summary> 
///     A collection of read-only strings used by the plugin.
/// </summary>
public static class PStrings
{
    /// <summary> 
    ///    This is the name that will be shown in all UI elements, does not change InternalName.
    /// </summary>
    public static readonly string pluginName = "Good Friend";

    /// <summary>
    ///     The repository to linked to this plugin.
    /// </summary>
    public static readonly string repoUrl = "https://github.com/BitsOfAByte/GoodFriend/";

    /// <summary>
    ///     The status page for the API, shown when API Url is the plugin default.
    ///     Leave as an empty string to disable showing this in the UI.
    /// </summary>
    public static readonly string statusPageUrl = "https://status.bitsofabyte.dev/status/goodfriend";

    /// <summary>
    ///     The production branch of the repository.
    /// </summary>
    public static readonly string repoBranch = "main";

    /// <summary>
    ///     The resources directory relative to the base of the repository.
    /// </summary>
    internal static readonly string repoResourcesDir = "src/GoodFriend.Plugin/Resources/";

    /// <summary>
    ///     The resources directory relative to the base of the repository.
    /// </summary>
    public static readonly string pluginResourcesDir = $"{PluginService.PluginInterface.AssemblyLocation.DirectoryName}\\Resources\\";

    /// <summary>
    ///     The path to the plugin's resources folder with trailing slashes, relative to the plugin assembly location with trailing slashes.
    /// </summary>
    public static readonly string pluginlocalizationDir = pluginResourcesDir + "Localization\\";

    /// <summary>
    ///     The support button URL.
    /// </summary>
    public static readonly string supportButtonUrl = "https://github.com/sponsors/BitsOfAByte";

    /// <summary>
    ///     The default API url to use.
    /// </summary>
    public static readonly Uri defaultAPIUrl = new Uri("https://aether.bitsofabyte.dev/");
}


/// <summary> 
///     A collection translatable strings.
/// </summary>
public static class TStrings
{
    public static string ConnectionSuccessful => Loc.Localize("Connection.Successful", "Successfully connected to the API.");
    public static string ConnectionError => Loc.Localize("Connection.Error", "An error occurred while connecting to the API.");
    public static string SettingsTabSettings => Loc.Localize("Settings.Tab.Settings", "Settings");
    public static string SettingsTabConnection => Loc.Localize("Settings.Tab.Connection", "Connection");
    public static string SettingsTabDebug => Loc.Localize("Settings.Tab.Debug", "Debug");
    public static string SettingsSupportText => Loc.Localize("Settings.Support", "Donate / Support");
    public static string SettingsViewStatusText => Loc.Localize("Settings.ViewStatus", "View Status");
    public static string SettingsAPIConnected => Loc.Localize("Settings.Connection.APIConnected", "You are connected to the API and recieving events");
    public static string SettingsAPIOnlineUsers(int i) => String.Format(Loc.Localize("Settings.Connection.APIOnlineUsers", "Online Users: {0}"), i);
    public static string SettingsAPIDisconnected => Loc.Localize("Settings.Connection.APIDisconnected", "You are not connected to the API are not recieving events.");
    public static string SettingsAPIDisconnectedDesc => Loc.Localize("Settings.Connection.APIDisconnectedDesc", "You should be automatically reconnected when the API becomes available, if you wish to reconnect manually you can try use the button below. For more information about why you're not connected, check /xllog");
    public static string SettingsAPITryReconnect => Loc.Localize("Settings.Connection.TryReconnect", "Attempt to reconnect");
    public static string SettingsAPINotLoggedIn => Loc.Localize("Settings.Connection.NotLoggedIn", "You must log in to a character to connect to the API");
    public static string SettingsAPILogTitle => Loc.Localize("Settings.Connection.Log", "Event Log");
    public static string SettingsAPILogDesc => Loc.Localize("Settings.Connections.Log.Desc", "You will see friend events in the log below when you recieve them.");
    public static string SettingsAPILogTime => Loc.Localize("Settings.Connection.Log.Time", "Time");
    public static string SettingsAPILogPlayer => Loc.Localize("Settings.Connection.Log.Player", "Player");
    public static string SettingsAPILogEvent => Loc.Localize("Settings.Connection.Log.Event", "Event");
    public static string SettingsAPILoginEvent => Loc.Localize("Settings.Connection.Log.Login", "Login");
    public static string SettingsAPILogoutEvent => Loc.Localize("Settings.Connection.Log.Logout", "Logout");
    public static string SettingsHideSameFC => Loc.Localize("Settings.HideSameFC", "Hide FC Members");
    public static string SettingsHideSameTCTooltip => Loc.Localize("Settings.HideSameFC.Tooltip", "When enabled, you will not be notified when a friend in your free company logs in or out.");
    public static string SettingsAPIEventNotifications => Loc.Localize("Settings.APIEventNotifications", "API Event Notifications");
    public static string SettingsAPIEventNotificationsTooltip => Loc.Localize("Settings.APIEventNotifications.Tooltip", "Show toast notifications for events from the API (eg. connection errors, etc)");
    public static string SettingsEnabled => Loc.Localize("Settings.Enabled", "Enabled");
    public static string SettingsDisabled => Loc.Localize("Settings.Disabled", "Disabled");
    public static string SettingsNotificationType => Loc.Localize("Settings.NotificationType", "Notification Type");
    public static string SettingsNotificationTypeTooltip => Loc.Localize("Settings.NotificationType.Tooltip", "The place to display notifications when a friend logs in or logs out.");
    public static string SettingsLoginMessage => Loc.Localize("Settings.LoginMessage", "Login Message");
    public static string SettingsLoginMessageTooltip => Loc.Localize("Settings.LoginMessage.Tooltip", "The message to show when a friend logs in.\n\n{0}: Name of the friend.");
    public static string SettingsLogoutMessage => Loc.Localize("Settings.LogoutMessage", "Logout Message");
    public static string SettingsLogoutMessageTooltip => Loc.Localize("Settings.LogoutMessage.Tooltip", "The message to show when a friend logs out.\n\n{0}: Name of the friend.");
    public static string SettingsFriendshipCode => Loc.Localize("Settings.FriendshipCode", "Friendship Code");
    public static string SettingsFriendshipCodeHint => Loc.Localize("Settings.FriendshipCode.Hint", "Leave empty to send notifications to all friends");
    public static string SettingsFriendshipCodeTooltip => Loc.Localize("Settings.FriendshipCode.Tooltip", "Your friend code determines which friends you send and recieve notifications with.");
    public static string SettingsShowAdvanced => Loc.Localize("Settings.ShowAdvanced", "Show Advanced Settings");
    public static string SettingsSaltMode => Loc.Localize("Settings.SaltMode", "Salt Mode");
    public static string SettingsSaltModeTooltip => Loc.Localize("Settings.SaltMode.Tooltip", "Strict: Validation is done using both Friend Code & Plugin Assembly (Both users must be on the same version of the plugin)\nRelaxed: Validation is only done using Friend Code (Users can recieve events from any plugin using the API)\n\nKeep this on strict for the best security.");
    public static string SettingsAPIURL => Loc.Localize("Settings.APIURL", "API URL");
    public static string SettingsAPIURLTooltip => Loc.Localize("Settings.APIURL.Tooltip", "The API that all events will be sent to and recieved from. Changes do not apply until the plugin has been reloaded.");
    public static string SettingsAPIToken => Loc.Localize("Settings.APIToken", "API Key");
    public static string SettingsAPITokenTooltip => Loc.Localize("Settings.APIToken.Tooltip", "The API bearer token to use when connecting to the API. Changes do not apply until the plugin has been reloaded.");
    public static string SettingsAPITokenHint => Loc.Localize("Settings.APIToken.Hint", "You can leave this empty unless you are facing authentication issues.");
}
