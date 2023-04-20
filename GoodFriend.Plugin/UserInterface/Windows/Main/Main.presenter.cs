using System;
using System.Collections.Generic;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Common;
using GoodFriend.Plugin.Game.Friends;
using GoodFriend.Plugin.Managers;

namespace GoodFriend.Plugin.UserInterface.Windows.Main
{
    internal sealed class MainPresenter : IDisposable
    {
        public void Dispose() { }

        /// <summary>
        ///     Stores which dropdown menu is currently open.
        /// </summary>
        public VisibleDropdown CurrentVisibleDropdown { get; set; } = VisibleDropdown.None;

        /// <summary>
        ///     Boolean value indicating if the user needs to restart to apply changes.
        /// </summary>
        public bool RestartToApply { get; set; }

        /// <summary>
        ///     The metadata cache instance to use in the UI.
        /// </summary>
        public static MetadataResponse? Metadata => Services.APIClientManager.MetadataCache;

        /// <summary>
        ///     Toggles the selected dropdown. If a dropdown of the same type is already open, the type is set to <see cref="VisibleDropdown.None"/>.
        /// </summary>
        /// <param name="dropdown"></param>
        public void ToggleVisibleDropdown(VisibleDropdown dropdown) => this.CurrentVisibleDropdown = dropdown == this.CurrentVisibleDropdown ? VisibleDropdown.None : dropdown;

        /// <summary>
        ///     The currently visible dropdown.
        /// </summary>
        public enum VisibleDropdown
        {
            None = 0,
            Donate = 1,
            Settings = 3,
        }

#if DEBUG
        /// <summary>
        ///     Fetches the users friendslist from the game.
        /// </summary>
        internal unsafe List<FriendListEntry> GetFriendList()
        {
            List<FriendListEntry> friends = new();
            foreach (FriendListEntry* friend in FriendListManager.Get())
            {
                friends.Add(*friend);
            }
            return friends;
        }
#endif
    }
}
