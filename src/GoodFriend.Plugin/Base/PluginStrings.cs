namespace GoodFriend.Base
{
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

    public static class Events
    {
        public static string APIConnectionStatus => Loc.Localize("Event.Connection.Status", "Connection Status");
        public static string APIConnectionSuccess => Loc.Localize("Event.Connection.Successful", "Successfully connected to the API");
        public static string APIConnectionError => Loc.Localize("Event.Connection.Error", "An error occurred while connecting to the API");
        public static string APIConnectionDisconnected => Loc.Localize("Event.Connection.Disconnected", "Disconnected from the API");
        public static string APIConnectionRatelimited => Loc.Localize("Event.Connection.Ratelimited", "Ratelimited by the API. Trying again at expiry time");
        public static string LoggedIn => Loc.Localize("Event.LoggedIn", "logged in");
        public static string LoggedOut => Loc.Localize("Event.LoggedOut", "logged out");
    }

    public static class State
    {
        public static string Connected => Loc.Localize("State.Connected", "Connected");
        public static string ConnectedDescription(int clients) => String.Format(Loc.Localize("State.Connected.Description", "There are currently {0} connected clients."), clients);
        public static string Connecting => Loc.Localize("State.Connecting", "Connecting");
        public static string ConnectingDescription => Loc.Localize("State.Connecting.Description", "Attempting to connect to the API... Please wait.");
        public static string Ratelimited => Loc.Localize("State.Ratelimited", "Ratelimited");
        public static string RatelimitedDescription => Loc.Localize("State.Ratelimited.Description", "The API has ratelimited this client temporarily.");
        public static string Disconnected => Loc.Localize("State.Disconnected", "Disconnected");
        public static string DisconnectedDescription => Loc.Localize("State.Disconnected.Description", "Login to connect to the API.");
        public static string ConnectionError => Loc.Localize("State.ConnectionError", "Connection Error");
        public static string ConnectionErrorDescription => Loc.Localize("State.ConnectionError.Description", "Something went wrong with the API connection.");
        public static string Unknown => Loc.Localize("State.Unknown", "Unknown");
        public static string UnknownDescription => Loc.Localize("State.Unknown.Description", "We're not sure what's going on here.");
    }

    public static class PrimaryWindow
    {
        public static string DropdownOptionsStatus => Loc.Localize("Window.Dropdown.Options.Status", "Status");
        public static string DropdownOptionsStatusTooltip(string url) => String.Format(Loc.Localize("Window.Dropdown.Options.Status.Tooltip", "View the status page of this API instance\n({0})"), url);
        public static string DropdownOptionsSupport => Loc.Localize("Window.Dropdown.Options.Support", "Donate");
        public static string DropdownOptionsSupportTooltip => Loc.Localize("Window.Dropdown.Options.Support.Tooltip", "Support the developer and API instance host");
        public static string DropdownOptionsEventLog => Loc.Localize("Window.Dropdown.Options.EventLog", "Event Log");
        public static string DropdownOptionsEventLogTooltip => Loc.Localize("Window.Dropdown.Options.EventLog.Tooltip", "View the event log");
        public static string DropdownOptionsSettings => Loc.Localize("Window.Dropdown.Options.Settings", "Settings");
        public static string DropdownOptionsSettingsTooltip => Loc.Localize("Window.Dropdown.Options.Settings.Tooltip", "Change the settings for this plugin");
        public static string DropdownSettingsEnabled => Loc.Localize("Window.Dropdown.Settings.Enabled", "Enabled");
        public static string DropdownSettingsDisabled => Loc.Localize("Window.Dropdown.Settings.Disabled", "Disabled");
        public static string DropdownSettingsRestartRequired => Loc.Localize("Window.Dropdown.Settings.RestartRequired", "Plugin restart required to apply some configuration changes.");
        public static string DropdownSettingsIgnoreFC => Loc.Localize("Window.Dropdown.Settings.IgnoreFC", "Ignore FC Members");
        public static string DropdownSettingsIgnoreFCTooltip => Loc.Localize("Window.Dropdown.Settings.IgnoreFC.Tooltip", "Whether or not to ignore free company friend notifications");
        public static string DropdownSettingsNotificationType => Loc.Localize("Window.Dropdown.Settings.NotificationType", "Notification Type");
        public static string DropdownSettingsNotificationTypeTooltip => Loc.Localize("Window.Dropdown.Settings.NotificationType.Tooltip", "The type of notification to show when a friend event occurs.");
        public static string DropdownSettingsLoginMessage => Loc.Localize("Window.Dropdown.Settings.LoginMessage", "Login Message");
        public static string DropdownSettingsLoginMessageTooltip => Loc.Localize("Window.Dropdown.Settings.LoginMessage.Tooltip", "The message to show when a friend logs in.\nUse {0} to insert the friend's name.");
        public static string DropdownSettingsLogoutMessage => Loc.Localize("Window.Dropdown.Settings.LogoutMessage", "Logout Message");
        public static string DropdownSettingsLogoutMessageTooltip => Loc.Localize("Window.Dropdown.Settings.LogoutMessage.Tooltip", "The message to show when a friend logs out.\nUse {0} to insert the friend's name.");
        public static string DropdownSettingsAPINotifications => Loc.Localize("Window.Dropdown.Settings.APINotifications", "API Notifications");
        public static string DropdownSettingsAPINotificationsTooltip => Loc.Localize("Window.Dropdown.Settings.APINotifications.Tooltip", "Whether or not to notifications when the API connection status changes.");
        public static string DropdownSettingsFriendshipCode => Loc.Localize("Window.Dropdown.Settings.FriendshipCode", "Friendship Code");
        public static string DropdownSettingsFriendshipCodeTooltip => Loc.Localize("Window.Dropdown.Settings.FriendshipCode.Tooltip", "Your friendship code limits who you send notifications to.\nYou will only send and recieve notifications with people who have the same code.\n\nLeave empty for the best experience.");
        public static string DropdownSettingsAPIUrl => Loc.Localize("Window.Dropdown.Settings.APIUrl", "API URL");
        public static string DropdownSettingsAPIUrlTooltip => Loc.Localize("Window.Dropdown.Settings.APIUrl.Tooltip", "The URL of the API instance to connect to. Make sure you trust the host of this instance.");
        public static string DropdownSettingsAPIToken => Loc.Localize("Window.Dropdown.Settings.APIToken", "API Token");
        public static string DropdownSettingsAPITokenTooltip => Loc.Localize("Window.Dropdown.Settings.APIToken.Tooltip", "The API bearer token to use when connecting to the API.\nThis is used for private or protected instances and does not normally need to be set.\n\nNEVER SET THIS TO YOUR PASSWORD.");
        public static string DropdownSettingsSaltMethod => Loc.Localize("Window.Dropdown.Settings.SaltMethod", "Salt Method");
        public static string DropdownSettingsSaltMethodTooltip => Loc.Localize("Window.Dropdown.Settings.SaltMethod.Tooltip", "Strict: Validation is done using both Friend Code & Plugin Assembly (Users must be using the same plugin and version to exchange events)\nRelaxed: Validation is only done using Friend Code (Users can exchange events from any plugin using the same API)\n\nKeep this on strict for the best privacy and security.");
        public static string DropdownSupportTitle => Loc.Localize("Window.Dropdown.Support.Title", "Support & Donations");
        public static string DropdownSupportFlavourText => Loc.Localize("Window.Dropdown.Support.FlavourText", "You can find ways to support either the plugin developer or API host here. Any support is greatly appreciated!");
        public static string DropdownSupportDonate => Loc.Localize("Window.Dropdown.Support.Button", "Donate");
        public static string DropdownSupportDeveloper => Loc.Localize("Window.Dropdown.Support.Developer", "Plugin/Server Developer");
        public static string DropdownSupportDeveloperDescription => Loc.Localize("Window.Dropdown.Support.Developer.Description", "The developer is responsible for the code that makes the Plugin and API work.");
        public static string DropdownSupportAPIHost => Loc.Localize("Window.Dropdown.Support.APIHost", "API Instance Host");
        public static string DropdownSupportAPIHostDescription => Loc.Localize("Window.Dropdown.Support.APIHost.Description", "The API instance host is responsible for running and maintaining the server that this plugin is connected to.");
        public static string DropdownSupportAPIHostInvalidUri => Loc.Localize("Window.Dropdown.Support.APIHost.InvalidUri", "The API instance host has provided an invalid support URL.");

    }
    public static class URLNagWindow
    {
        public static string URLUpdateNagTitle(Uri url) => String.Format(Loc.Localize("Window.URLUpdateNag.Title", "{0} - Update from {1}"), PStrings.pluginName, url);
        public static string URLUpdateNagText(Uri? newUrl, int waitTime) => String.Format(Loc.Localize("Window.URLUpdateNag.Text", "Your configured instance is recommending that you update to a new API URL; this is likely due to a change in the instance's domain name or hosting provider.\n\nIf you trust the new URL({0}), press the button below to update the API URL for next plugin load. Otherwise, you may want to find another instance provider.\n\nHold shift or wait {1} seconds to select an option."), newUrl?.ToString() ?? "null", waitTime);
        public static string URLUpdateNagButtonUpdate => Loc.Localize("Window.URLUpdateNag.Button.Update", "Update URL");
        public static string URLUpdateNagButtonIgnore => Loc.Localize("Window.URLUpdateNag.Button.Ignore", "Ignore for this session");
    }
}