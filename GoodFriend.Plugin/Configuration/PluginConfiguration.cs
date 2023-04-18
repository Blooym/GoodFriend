using System;
using Dalamud.Configuration;
using GoodFriend.Plugin.Common;
using GoodFriend.Plugin.Utils;

namespace GoodFriend.Plugin.Configuration
{
    /// <summary>
    ///     Provides access to and determines the Plugin configuration.
    /// </summary>
    [Serializable]
    public sealed class PluginConfiguration : IPluginConfiguration
    {
        /// <summary>
        ///     The current configuration version, incremented on breaking changes.
        /// </summary>
        public int Version { get; set; }

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
        ///     Whether or not to hide notifications from users from different homeworlds.
        /// </summary>
        public bool HideDifferentHomeworld { get; set; }

        /// <summary>
        ///     Whether or not to hide notifications from users in different territories.
        /// </summary>
        public bool HideDifferentTerritory { get; set; }

        /// <summary>
        ///     Whether or not to hide notifications from users in different worlds.
        /// </summary>
        public bool HideDifferentWorld { get; set; }

        /// <summary>
        ///     Whether or not to hide notifications from users in different data centers.
        /// </summary>
        public bool HideDifferentDatacenter { get; set; }

        /// <summary>
        ///    Whether or not to cache the friendslist for when it is unavailable (eg. instanced content).
        /// </summary>
        public bool FriendslistCaching { get; set; } = true;

        /// <summary>
        ///     Whether or not to show API events as notifications.
        /// </summary>
        public bool ShowAPIEvents { get; set; }

        /// <summary>
        ///     The BaseURL to use when interacting with the API.
        /// </summary>
        public Uri APIUrl { get; set; } = Constants.Links.DefaultAPIUrl;

        /// <summary>
        ///     Saves the current configuration to disk.
        /// </summary>
        internal void Save() => Services.PluginInterface.SavePluginConfig(this);
    }
}
