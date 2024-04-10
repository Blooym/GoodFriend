#if DEBUG
using System;
using Dalamud.Memory;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.Utility;
using ImGuiNET;
using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens;

internal static class DebugScreen
{
    /// <summary>
    ///     The current debug option.
    /// </summary>
    private static DebugOption currentOption = DebugOption.Friends;

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
        SiGui.Heading(Strings.UI_MainWindow_DebugScreen_DetectedFriends);
        SiGui.TextWrapped(Strings.UI_MainWindow_DebugScreen_DetectedFriends_Usage);
        ImGui.Dummy(Spacing.SectionSpacing);

        if (ImGui.BeginChild("DebugFriendList"))
        {
            foreach (var f in FriendHelper.FriendList)
            {
                if (f.ExtraFlags == Constants.WaitingForFriendListApproval)
                {
                    continue;
                }

                var fName = MemoryHelper.ReadSeStringNullTerminated((nint)f.Name).ToString();
                if (string.IsNullOrEmpty(fName))
                {
                    continue;
                }

                if (ImGui.Selectable(fName))
                {
                    var salt = CryptoUtil.GenerateSalt();
                    var hash = CryptoUtil.HashValue(f.ContentId, salt);
                    ImGui.SetClipboardText($"Friend: {fName} Hash: {hash} | Salt: {salt}");
                    ChatHelper.Print(string.Format(Strings.UI_MainWindow_DebugScreen_DetectedFriends_Copied, fName), (ushort)ChatUiColourKey.LightPurple1);
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
#endif
