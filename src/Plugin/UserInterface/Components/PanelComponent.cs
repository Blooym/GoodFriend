using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace GoodFriend.Plugin.UserInterface.Components;

internal static class PanelComponent
{
    /// <summary>
    ///     Draws two panels side by side.
    /// </summary>
    /// <param name="id">The unique identifier for this split panel</param>
    /// <param name="leftWidth">The width of the left panel.</param>
    /// <param name="rightWidth">The width of the right panel.</param>
    /// <param name="leftContent">The content of the left panel.</param>
    /// <param name="rightContent">The content of the right panel.</param>
    public static void DrawSplitPanels(string id, float leftWidth, float rightWidth, Action leftContent, Action rightContent)
    {
        using (var table = ImRaii.Table($"SplitPanel##{id}", 2))
        {
            if (table.Success)
            {
                ImGui.TableSetupColumn($"SplitPanelLeft##{id}", ImGuiTableColumnFlags.WidthFixed, leftWidth);
                ImGui.TableSetupColumn($"SplitPanelRight##{id}", ImGuiTableColumnFlags.WidthFixed, rightWidth);
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                using (var child = ImRaii.Child($"LeftPanelChild##{id}", default, true))
                {
                    if (child.Success)
                    {
                        leftContent();
                    }
                }
                ImGui.TableNextColumn();
                using (var child = ImRaii.Child($"RightPanelChild##{id}", default, true))
                {
                    if (child.Success)
                    {
                        rightContent();
                    }
                }
            }
        }
    }
}
