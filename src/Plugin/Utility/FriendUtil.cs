using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Sirensong.Game.Helpers;

namespace GoodFriend.Plugin.Utility;

internal static class FriendUtil
{
    /// <summary>
    ///     Gets a friend by their content ID hash and the salt used to hash it.
    /// </summary>
    /// <param name="contentIdHash">The content ID hash to search for.</param>
    /// <param name="contentIdSalt">The salt used to hash the original content ID.</param>
    /// <returns>The friend's CharacterData if matched.</returns>
    public static InfoProxyCommonList.CharacterData? GetFriendFromHash(string contentIdHash, string contentIdSalt)
    {
        foreach (var friend in FriendHelper.FriendList)
        {
            if (CryptoUtil.HashValue(friend.ContentId, contentIdSalt) == contentIdHash)
            {
                return friend;
            }
        }
        return null;
    }
}
