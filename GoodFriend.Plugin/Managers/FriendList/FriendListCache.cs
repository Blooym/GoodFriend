namespace GoodFriend.Plugin.Managers.FriendList
{
    /// <summary>
    ///     The class containing friend list cache functionality
    /// </summary>
    public static class FriendListCache
    {
        private static unsafe FriendListEntry*[]? cache;

        /// <summary>
        ///     A list of the currently-logged-in player's friends, returns cached version if list is empty and the player is logged in.
        /// </summary>
        public static unsafe FriendListEntry*[] Get()
        {
            if (Services.ClientState.LocalContentId == 0)
            {
                return new FriendListEntry*[0];
            }

            var friendList = FriendList.Get();

            // If caching of friends has been disabled, return the current friend list.
            if (!Services.Configuration.FriendslistCaching)
            {
                return friendList;
            }

            // Otherwise, if the current friend list is not empty, update the cache.
            if (friendList.Length != 0)
            {
                cache = friendList;
            }

            return cache ?? new FriendListEntry*[0];
        }
    }
}
