#if DEBUG
using System;
using Dalamud.Memory;
using GoodFriend.Plugin.Utility;
using ImGuiNET;
using Sirensong;
using Sirensong.Cache;
using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens
{
    internal static class DebugScreen
    {
        /// <summary>
        ///     The current debug option.
        /// </summary>
        private static DebugOption currentOption = DebugOption.Friends;

        /// <summary>
        ///     The friend list cache.
        /// </summary>
        private static FriendListCache FriendCache { get; } = SirenCore.GetOrCreateService<FriendListCache>();

        /// <summary>
        ///     Draws the debug option list.
        /// </summary> 
        public static void DrawDebugList()
        {
            foreach (var setting in Enum.GetValues<DebugOption>())
            {
                if (ImGui.Selectable(setting.ToString(), setting == currentOption))
                {
                    currentOption = setting;
                }
            }
        }

        /// <summary>
        ///     Draws the debug details.
        /// </summary>
        public static void DrawDebugDetails()
        {
            switch (currentOption)
            {
                case DebugOption.Friends:
                    DrawApiDebug();
                    break;
            }
        }

        /// <summary>
        ///     Draws the API debug screen.
        /// </summary>
        private static unsafe void DrawApiDebug()
        {
            SiGui.Heading("Detected Friends");
            SiGui.TextWrapped("Click on a friend to copy their content id hash and salt");
            ImGui.Dummy(Spacing.SectionSpacing);

            if (ImGui.BeginChild("DebugFriendList"))
            {
                foreach (var f in FriendCache.List)
                {
                    var fName = MemoryHelper.ReadSeStringNullTerminated((nint)f.Name);
                    if (ImGui.Selectable(fName.ToString()))
                    {
                        var salt = CryptoUtil.GenerateSalt();
                        var hash = CryptoUtil.HashValue(f.ContentId, salt);
                        ImGui.SetClipboardText($"Friend: {fName} Hash: {hash} | Salt: {salt}");
                        ChatHelper.Print($"Copied {fName}'s data to clipboard", (ushort)ChatUiColourKey.LightPurple1);
                    }
                }
            }
            ImGui.EndChild();
        }

        private enum DebugOption
        {
            Friends,
        }
    }
}
#endif