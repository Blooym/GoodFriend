using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem.Modules;
using GoodFriend.Plugin.ModuleSystem.Modules.Required;
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
                    if (lastTag != null)
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

                if (module.GetType().BaseType == typeof(RequiredModuleBase))
                {
                    continue;
                }

                switch (module.State)
                {
                    case ModuleState.Enabled:
                        var enabledText = Strings.UI_MainWindow_ModuleScreen_Enabled;
                        SiGui.TextColoured(Colours.Success, enabledText);
                        break;
                    case ModuleState.Loading:
                        var loadingText = Strings.UI_MainWindow_ModuleScreen_Loading;
                        SiGui.TextColoured(Colours.Warning, loadingText);
                        break;
                    case ModuleState.Disabled:
                        var disabledText = Strings.UI_MainWindow_ModuleScreen_Disabled;
                        SiGui.TextColoured(ImGuiColors.DalamudGrey, disabledText);
                        break;
                    case ModuleState.Unloading:
                        var unloadingText = Strings.UI_MainWindow_ModuleScreen_Unloading;
                        SiGui.TextColoured(Colours.Warning, unloadingText);
                        break;
                    case ModuleState.Error:
                        var errorText = Strings.UI_MainWindow_ModuleScreen_Error;
                        SiGui.TextColoured(Colours.Error, errorText);
                        break;
                }
            }
        }

        /// <summary>
        ///     Draw the detail panel for the selected module.
        /// </summary>
        public static void DrawModuleDetails()
        {
            if (CurrentModule == null)
            {
                ImGui.SetCursorPos(new Vector2((ImGui.GetContentRegionAvail().X / 2) - (ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_SelectToView).X / 2), (ImGui.GetContentRegionAvail().Y / 2) - (ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_SelectToView).Y / 2)));
                SiGui.TextDisabledWrapped(Strings.UI_MainWindow_ModuleScreen_SelectToView);
                return;
            }

            SiGui.Heading(CurrentModule.Name);
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
