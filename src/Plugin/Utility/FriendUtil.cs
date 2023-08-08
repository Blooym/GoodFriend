using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Sirensong;
using Sirensong.Cache;

namespace GoodFriend.Plugin.Utility
{
    internal static class FriendUtil
    {
        /// <summary>
        ///     The friend list cache.
        /// </summary>
        private static readonly FriendListCache FriendListCache = SirenCore.GetOrCreateService<FriendListCache>();

        /// <summary>
        ///     Get the friends list, either cached or uncached depending on if it is currently available.
        /// </summary>
        /// <returns>The players current friends or empty if not cached & unavailable.</returns>
        public static IEnumerable<InfoProxyCommonList.CharacterData> GetFriendsList() => FriendListCache.List;

        /// <summary>
        ///     Gets a friend by their content ID hash and the salt used to hash it.
        /// </summary>
        /// <param name="contentIdHash">The content ID hash to search for.</param>
        /// <param name="contentIdSalt">The salt used to hash the original content ID.</param>
        /// <returns>The friend if found, otherwise null.</returns>
        public static InfoProxyCommonList.CharacterData? GetFriendByHash(string contentIdHash, string contentIdSalt)
        {
            foreach (var friend in FriendListCache.List)
            {
                if (CryptoUtil.HashValue(friend.ContentId, contentIdSalt) == contentIdHash)
                {
                    return friend;
                }
            }
            return null;
        }
    }
}
