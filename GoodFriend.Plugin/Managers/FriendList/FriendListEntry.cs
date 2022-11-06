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
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;

namespace GoodFriend.Managers.FriendList
{
    /// <summary>
    ///     An entry in a player's friend list.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    public unsafe struct FriendListEntry
    {
        internal const int Size = 96;

        /// <summary>
        ///     The content ID of the friend.
        /// </summary>
        [FieldOffset(0)]
        public readonly ulong ContentId;

        [FieldOffset(13)]
        public readonly byte OnlineStatus;

        /// <summary>
        ///     The current world of the friend.
        /// </summary>
        [FieldOffset(22)]
        public readonly ushort CurrentWorld;

        /// <summary>
        ///     The home world of the friend.
        /// </summary>
        [FieldOffset(24)]
        public readonly ushort HomeWorld;

        /// <summary>
        ///     The job the friend is currently on.
        /// </summary>
        [FieldOffset(33)]
        public readonly byte Job;

        /// <summary>
        ///     The friend's raw SeString name. See <see cref="Name"/>.
        /// </summary>
        [FieldOffset(34)]
        public fixed byte RawName[32];

        /// <summary>
        ///     The friend's raw SeString free company tag. See <see cref="FreeCompany"/>.
        /// </summary>
        [FieldOffset(66)]
        public fixed byte RawFreeCompany[5];

        /// <summary>
        ///     The friend's name.
        /// </summary>
        public SeString Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr);
                }
            }
        }

        /// <summary>
        ///     The friend's free company tag.
        /// </summary>
        public SeString FreeCompany
        {
            get
            {
                fixed (byte* ptr = RawFreeCompany)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr);
                }
            }
        }

        public bool IsOnline => OnlineStatus == 0x80;
    }
}