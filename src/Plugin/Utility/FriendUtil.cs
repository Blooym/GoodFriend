using System;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace GoodFriend.Plugin.Utility;

internal static class FriendUtil
{
    /// <summary>
    ///     Gets a friend by their content ID hash and the salt used to hash it.
    /// </summary>
    /// <param name="friendList">The friend list to check hashes on</param>
    /// <param name="contentIdHash">The content ID hash to search for.</param>
    /// <param name="contentIdSalt">The salt used to hash the original content ID.</param>
    /// <returns>The friend's CharacterData if matched.</returns>
    public static InfoProxyCommonList.CharacterData? GetFriendFromHash(ReadOnlySpan<InfoProxyCommonList.CharacterData> friendList, string contentIdHash, string contentIdSalt)
    {
        foreach (var friend in friendList)
        {
            if (CryptoUtil.HashValue(friend.ContentId, contentIdSalt) == contentIdHash)
            {
                return friend;
            }
        }
        return null;
    }
}
