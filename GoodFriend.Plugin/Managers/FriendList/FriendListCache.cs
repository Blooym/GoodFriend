using System;
using System.Collections.Generic;
using GoodFriend.Base;
using XivCommon.Functions.FriendList;

namespace GoodFriend.Managers.FriendList
{
    /// <summary>
    ///     The class containing friend list cache functionality
    /// </summary>
    public static class FriendListCache
    {
        private static unsafe IList<FriendListEntry>? cache;

        /// <summary>
        ///     A list of the currently-logged-in player's friends, returns cached version if list is empty and the player is logged in.
        /// </summary>
        public static unsafe IList<FriendListEntry> Get()
        {
            if (PluginService.ClientState.LocalContentId == 0)
            {
                return Array.Empty<FriendListEntry>();
            }

            var friendList = PluginService.XivCommon.Functions.FriendList.List;

            // If caching of friends has been disabled, return the current friend list.
            if (!PluginService.Configuration.FriendslistCaching)
            {
                return friendList;
            }

            // Otherwise, if the current friend list is not empty, update the cache.
            if (friendList.Count != 0)
            {
                cache = friendList;
            }

            return cache ?? Array.Empty<FriendListEntry>();
        }
    }
}
