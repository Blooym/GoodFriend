using System;
using ImGuiNET;

namespace GoodFriend.Plugin.UserInterface.Components
{
    internal static class PanelComponent
    {
        /// <summary>
        ///     Draws two panels side by side.
        /// </summary>
        /// <param name="leftWidth">The width of the left panel.</param>
        /// <param name="rightWidth">The width of the right panel.</param>
        /// <param name="leftContent">The content of the left panel.</param>
        /// <param name="rightContent">The content of the right panel.</param>
        public static void DrawSplitPanels(float leftWidth, float rightWidth, Action leftContent, Action rightContent)
        {
            if (ImGui.BeginTable("SettingsPanelTable", 2))
            {
                ImGui.TableSetupColumn("SettingsLeftPanel", ImGuiTableColumnFlags.WidthFixed, leftWidth);
                ImGui.TableSetupColumn("SettingsRightPanel", ImGuiTableColumnFlags.WidthFixed, rightWidth);
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.BeginChild("SettingsLeftPanelChild", default, true))
                {
                    leftContent();
                }
                ImGui.EndChild();

                ImGui.TableNextColumn();
                if (ImGui.BeginChild("SettingsRightPanelChild", default, true))
                {
                    rightContent();
                }
                ImGui.EndChild();
                ImGui.EndTable();
            }
        }
    }
}
