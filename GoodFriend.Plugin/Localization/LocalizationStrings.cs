using CheapLoc;

namespace GoodFriend.Plugin.Localization
{
    internal static class Events
    {
        public static string APIConnectionStatus => Loc.Localize("Event.Connection.Status", "Connection Status");
        public static string APIConnectionSuccess => Loc.Localize("Event.Connection.Successful", "Successfully connected to the API.");
        public static string APIConnectionError => Loc.Localize("Event.Connection.Error", "An error occurred while connecting to the API.");
        public static string APIConnectionDisconnected => Loc.Localize("Event.Connection.Disconnected", "Disconnected from the API.");
        public static string LoggedIn => Loc.Localize("Event.LoggedIn", "logged in.");
        public static string LoggedOut => Loc.Localize("Event.LoggedOut", "logged out.");
    }

    internal static class State
    {
        public static string Connected => Loc.Localize("State.Connected", "Connected");
        public static string ConnectedDescription(uint clients) => string.Format(Loc.Localize("State.Connected.Description", "There are currently {0} connected clients."), clients);
        public static string Connecting => Loc.Localize("State.Connecting", "Connecting");
        public static string ConnectingDescription => Loc.Localize("State.Connecting.Description", "Connection in progress, please wait.");
        public static string Disconnected => Loc.Localize("State.Disconnected", "Disconnected");
        public static string DisconnectedDescription => Loc.Localize("State.Disconnected.Description", "Login to connect to the API.");
        public static string ConnectionError => Loc.Localize("State.ConnectionError", "Connection Error");
        public static string ConnectionErrorDescription => Loc.Localize("State.ConnectionError.Description", "Something went wrong with the API connection.");
        public static string Unknown => Loc.Localize("State.Unknown", "Unknown");
        public static string UnknownDescription => Loc.Localize("State.Unknown.Description", "We're not sure what's going on here.");
    }

    internal static class PrimaryWindow
    {
        public static string DropdownOptionsStatus => Loc.Localize("Window.Dropdown.Options.Status", "Status");
        public static string DropdownOptionsStatusTooltip(string url) => string.Format(Loc.Localize("Window.Dropdown.Options.Status.Tooltip", "View the status page of this API instance.\n({0})"), url);
        public static string DropdownOptionsSupport => Loc.Localize("Window.Dropdown.Options.Support", "Donate");
        public static string DropdownOptionsSupportTooltip => Loc.Localize("Window.Dropdown.Options.Support.Tooltip", "Support the developer and API instance host.");
        public static string DropdownOptionsSettings => Loc.Localize("Window.Dropdown.Options.Settings", "Settings");
        public static string DropdownOptionsSettingsTooltip => Loc.Localize("Window.Dropdown.Options.Settings.Tooltip", "Change the settings for this plugin.");
        public static string DropdownSettingsEnabled => Loc.Localize("Window.Dropdown.Settings.Enabled", "Enabled");
        public static string DropdownSettingsDisabled => Loc.Localize("Window.Dropdown.Settings.Disabled", "Disabled");
        public static string DropdownSettingsRestartRequired => Loc.Localize("Window.Dropdown.Settings.RestartRequired", "Plugin restart required to apply some configuration changes.");
        public static string DropdownSettingsIgnoreFC => Loc.Localize("Window.Dropdown.Settings.IgnoreFC", "Ignore FC Members");
        public static string DropdownSettingsIgnoreFCTooltip => Loc.Localize("Window.Dropdown.Settings.IgnoreFC.Tooltip", "Whether or not to ignore friend notifications from players in your Free Company, may not work inside of instances or worlds other than your homeworld.");
        public static string DropdownSettingsIgnoreDiffHomeworlds => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffHomeworlds", "Ignore Other Homeworlds");
        public static string DropdownSettingsIgnoreDiffHomeworldsTooltip => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffHomeworlds.Tooltip", "Whether or not to ignore friend notifications from player characters who have a different homeworld than you.");
        public static string DropdownSettingsIgnoreDiffWorlds => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffWorlds", "Ignore Other Worlds");
        public static string DropdownSettingsIgnoreDiffWorldsTooltip => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffWorlds.Tooltip", "Whether or not to ignore friend notifications from players currently in different worlds than you.");
        public static string DropdownSettingsIgnoreDiffTerritories => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffTerritories", "Ignore Other Zones");
        public static string DropdownSettingsIgnoreDiffTerritoriesTooltip => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffTerritories.Tooltip", "Whether or not to ignore friend notifications from players currently in different zones.");
        public static string DropdownSettingsIgnoreDiffDatacenters => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffDatacenters", "Ignore Other Datacenters");
        public static string DropdownSettingsIgnoreDiffDatacentersTooltip => Loc.Localize("Window.Dropdown.Settings.IgnoreDiffDatacenters.Tooltip", "Whether or not to ignore friend notifications from players currently on different datacenters.");
        public static string DropdownSettingsNotificationType => Loc.Localize("Window.Dropdown.Settings.NotificationType", "Notification Type");
        public static string DropdownSettingsNotificationTypeTooltip => Loc.Localize("Window.Dropdown.Settings.NotificationType.Tooltip", "The type of notification to show when a friend event occurs.");
        public static string DropdownSettingsLoginMessage => Loc.Localize("Window.Dropdown.Settings.LoginMessage", "Login Message");
        public static string DropdownSettingsLoginMessageTooltip => Loc.Localize("Window.Dropdown.Settings.LoginMessage.Tooltip", "The message to show when a friend logs in.\nMessage must contain {0} at least once to insert the friend's name.");
        public static string DropdownInvalidLoginMessage => Loc.Localize("Window.Dropdown.Settings.InvalidLoginMessage", "The login message must contain {0} at least once to insert the friend's name.");
        public static string DropdownSettingsLogoutMessage => Loc.Localize("Window.Dropdown.Settings.LogoutMessage", "Logout Message");
        public static string DropdownSettingsLogoutMessageTooltip => Loc.Localize("Window.Dropdown.Settings.LogoutMessage.Tooltip", "The message to show when a friend logs out.\nMessage must contain {0} at least once to insert the friend's name.");
        public static string DropdownInvalidLogoutMessage => Loc.Localize("Window.Dropdown.Settings.InvalidLogoutMessage", "The logout message must contain {0} at least once to insert the friend's name.");
        public static string DropdownSettingsAPINotifications => Loc.Localize("Window.Dropdown.Settings.APINotifications", "API Notifications");
        public static string DropdownSettingsAPINotificationsTooltip => Loc.Localize("Window.Dropdown.Settings.APINotifications.Tooltip", "Whether or not to notifications when the API connection status changes, this is not related to friend notifications.");
        public static string DropdownSettingsAPIUrl => Loc.Localize("Window.Dropdown.Settings.APIUrl", "API URL");
        public static string DropdownSettingsAPIUrlTooltip => Loc.Localize("Window.Dropdown.Settings.APIUrl.Tooltip", "The URL of the API instance to connect to. You should trust the host of this instance before connecting to it.");
        public static string DropdownSettingsAPIUrlInvalid => Loc.Localize("Window.Dropdown.Settings.APIUrlInvalid", "The API URL is invalid, please double check it contains the protocol (http:// or https://) and the port (if applicable).");
        public static string DropdownSettingsAPIToken => Loc.Localize("Window.Dropdown.Settings.APIToken", "API Token");
        public static string DropdownSettingsAPITokenTooltip => Loc.Localize("Window.Dropdown.Settings.APIToken.Tooltip", "Sets the 'Authorization' header value to send with all requests to the API.\nYou will need to include the authorization scheme (eg. bearer, basic) before the value.\nThe API instance will be able to see this value, never set it to sensitive information like your login password.");
        public static string DropdownSettingsFriendslistCaching => Loc.Localize("Window.Dropdown.Settings.FriendslistCaching", "Friendslist Caching");
        public static string DropdownSettingsFriendslistCachingTooltip => Loc.Localize("Window.Dropdown.Settings.FriendslistCaching.Tooltip", "Whether or not to cache the friends list to show notifications when it is unavailable (eg. inside of instanced content).\nDisable to not show notifications when the friends list is unavailable.\n\nNote: When enabling this setting, the cache will not be updated until the next time the friends list is available.");
        public static string DropdownSupportTitle => Loc.Localize("Window.Dropdown.Support.Title", "Support & Donations");
        public static string DropdownSupportFlavourText => Loc.Localize("Window.Dropdown.Support.FlavourText", "You can find ways to support either the plugin developer or API host here. Any support is greatly appreciated!");
        public static string DropdownSupportDonate => Loc.Localize("Window.Dropdown.Support.Button", "Donate");
        public static string DropdownSupportDeveloper => Loc.Localize("Window.Dropdown.Support.Developer", "Plugin/Server Developer");
        public static string DropdownSupportDeveloperDescription => Loc.Localize("Window.Dropdown.Support.Developer.Description", "The developer is responsible for the code that makes the Plugin and API work.");
        public static string DropdownSupportAPIHost => Loc.Localize("Window.Dropdown.Support.APIHost", "API Instance Host");
        public static string DropdownSupportAPIHostDescription => Loc.Localize("Window.Dropdown.Support.APIHost.Description", "The API instance host is responsible for running and maintaining the server that this plugin is connected to.");
        public static string DropdownSupportAPIHostInvalidUri => Loc.Localize("Window.Dropdown.Support.APIHost.InvalidUri", "The API instance host has provided an invalid support URL.");
        public static string SettingsTabGeneral => Loc.Localize("Window.Settings.Tab.General", "General");
        public static string SettingsTabAdvanced => Loc.Localize("Window.Settings.Tab.Advanced", "Advanced");
    }
}
