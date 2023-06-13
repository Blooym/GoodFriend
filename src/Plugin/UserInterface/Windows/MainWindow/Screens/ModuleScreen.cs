using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem.Modules;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens
{
    // TODO: cleanup the gui code here.
    internal static class ModuleScreen
    {
        /// <summary>
        ///     The current module.
        /// </summary>
        private static ModuleBase? CurrentModule { get; set; } = Services.ModuleService.GetModule<InstanceInfoModule>();

        /// <summary>
        ///     Draws the list panel of the main window.
        /// </summary>
        public static void DrawModuleList()
        {
            ModuleTag? lastTag = null;
            foreach (var module in Services.ModuleService.GetModules().OrderBy(x => x.Tag).OrderByDescending(x => x.DisplayWeight))
            {
                if (lastTag != module.Tag)
                {
                    if (lastTag is not null)
                    {
                        ImGui.Dummy(Spacing.SidebarSectionSpacing);
                    }
                    lastTag = module.Tag;
                    SiGui.Heading(module.Tag.ToString());
                }
                else
                {
                    ImGui.Dummy(new Vector2(0, 1.5f));
                }

                if (ImGui.Selectable(module.Name, CurrentModule == module))
                {
                    CurrentModule = module;
                }
            }
        }

        /// <summary>
        ///     Draw the detail panel for the selected module.
        /// </summary>
        public static void DrawModuleDetails()
        {
            if (CurrentModule is null)
            {
                ImGui.SetCursorPos(new Vector2((ImGui.GetContentRegionAvail().X / 2) - (ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_SelectToView).X / 2), (ImGui.GetContentRegionAvail().Y / 2) - (ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_SelectToView).Y / 2)));
                SiGui.TextDisabledWrapped(Strings.UI_MainWindow_ModuleScreen_SelectToView);
                return;
            }

            SiGui.TextDisabledWrapped(CurrentModule.Name);
            ImGui.SameLine();
            switch (CurrentModule.State)
            {
                case ModuleState.Enabled:
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_Enabled).X);
                    SiGui.TextColoured(Colours.Success, Strings.UI_MainWindow_ModuleScreen_Enabled);
                    break;
                case ModuleState.Loading:
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_Loading).X);
                    SiGui.TextColoured(Colours.Warning, Strings.UI_MainWindow_ModuleScreen_Loading);
                    break;
                case ModuleState.Disabled:
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_Disabled).X);
                    SiGui.TextColoured(ImGuiColors.DalamudGrey, Strings.UI_MainWindow_ModuleScreen_Disabled);
                    break;
                case ModuleState.Unloading:
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_Unloading).X);
                    SiGui.TextColoured(Colours.Warning, Strings.UI_MainWindow_ModuleScreen_Unloading);
                    break;
                case ModuleState.Error:
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_Error).X);
                    SiGui.TextColoured(Colours.Error, Strings.UI_MainWindow_ModuleScreen_Error);
                    break;
            }
            ImGui.Separator();
            ImGui.Dummy(Spacing.HeaderSpacing);

            if (!string.IsNullOrEmpty(CurrentModule.Description))
            {
                SiGui.TextWrapped(CurrentModule.Description);
                ImGui.Dummy(Spacing.SectionSpacing);
            }
            if (ImGui.BeginChild("PluginSettingsListChildScrolling"))
            {
                CurrentModule.Draw();
            }
            ImGui.EndChild();
        }
    }
}
