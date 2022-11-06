/**
    MIT License

    Copyright (c) 2021 Anna Clemens

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/
using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace GoodFriend.Managers.FriendList
{
    /// <summary>
    ///     The class containing friend list functionality
    /// </summary>
    public static class FriendList
    {
        // Updated: 5.58-HF1
        private const int InfoOffset = 0x28;
        private const int LengthOffset = 0x10;
        private const int ListOffset = 0x98;
        /// <summary>
        /// <para>
        ///     A live list of the currently-logged-in player's friends.
        /// </para>
        /// <para>
        ///     The list is empty if not logged in.
        /// </para>
        /// </summary>
        public static unsafe FriendListEntry*[] Get()
        {
            var friendListAgent = (IntPtr)Framework.Instance()
                ->GetUiModule()
                ->GetAgentModule()
                ->GetAgentByInternalId(AgentId.SocialFriendList);
            if (friendListAgent == IntPtr.Zero)
            {
                return new FriendListEntry*[] { };
            }
            var info = *(IntPtr*)(friendListAgent + InfoOffset);
            if (info == IntPtr.Zero)
            {
                return new FriendListEntry*[] { };
            }
            var length = *(ushort*)(info + LengthOffset);
            if (length == 0)
            {
                return new FriendListEntry*[] { };
            }
            var list = *(IntPtr*)(info + ListOffset);
            if (list == IntPtr.Zero)
            {
                return new FriendListEntry*[] { };
            }
            var entries = new FriendListEntry*[length];
            for (var i = 0; i < length; i++)
            {
                var entry = (FriendListEntry*)(list + (i * FriendListEntry.Size));
                entries[i] = entry;
            }
            return entries;
        }
    }
}
