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

namespace GoodFriend.Plugin.Game.Friends
{
    /// <summary>
    ///     An entry in a player's friend list.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    internal unsafe struct FriendListEntry
    {
        internal const int Size = 96;

        /// <summary>
        ///     The content ID of the friend.
        /// </summary>
        [FieldOffset(0)]
        internal readonly ulong ContentId;

        [FieldOffset(13)]
        internal readonly byte OnlineStatus;

        /// <summary>
        ///     The current world of the friend.
        /// </summary>
        [FieldOffset(22)]
        internal readonly ushort CurrentWorld;

        /// <summary>
        ///     The home world of the friend.
        /// </summary>
        [FieldOffset(24)]
        internal readonly ushort HomeWorld;

        /// <summary>
        ///     The job the friend is currently on.
        /// </summary>
        [FieldOffset(33)]
        internal readonly byte Job;

        /// <summary>
        ///     The friend's raw SeString name. See <see cref="Name"/>.
        /// </summary>
        [FieldOffset(34)]
        internal fixed byte RawName[32];

        /// <summary>
        ///     The friend's raw SeString free company tag. See <see cref="FreeCompany"/>.
        /// </summary>
        [FieldOffset(66)]
        internal fixed byte RawFreeCompany[5];

        /// <summary>
        ///     The friend's name.
        /// </summary>
        internal readonly SeString Name
        {
            get
            {
                fixed (byte* ptr = this.RawName)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr);
                }
            }
        }

        /// <summary>
        ///     The friend's free company tag.
        /// </summary>
        internal readonly SeString FreeCompany
        {
            get
            {
                fixed (byte* ptr = this.RawFreeCompany)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr);
                }
            }
        }

        internal bool IsOnline => this.OnlineStatus == 0x80;
    }
}
