using GoodFriend.Plugin.Common;

namespace GoodFriend.Plugin.Game.Friends
{
    /// <summary>
    ///     Manager for player friend list functionality.
    /// </summary>
    internal static class FriendListManager
    {
        /// <summary>
        /// The currently cached player friend-list.
        /// </summary>
        private static unsafe FriendListEntry*[]? cache;

        /// <summary>
        ///     Get the current players friends list, or a cached version if empty.
        /// </summary>
        internal static unsafe FriendListEntry*[] Get()
        {
            // Not logged in so player would have no friends.
            if (Services.ClientState.LocalContentId == 0)
            {
                return new FriendListEntry*[0];
            }

            // Grab the FriendList from the game.
            var friendList = FriendList.Get();

            // If the players FriendsList isn't empty, then cache it for when it is.
            if (friendList.Length != 0)
            {
                cache = friendList;
            }

            return cache ?? new FriendListEntry*[0];
        }
    }
}
