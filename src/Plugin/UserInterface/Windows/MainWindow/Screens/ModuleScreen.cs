using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using GoodFriend.Plugin.Base;
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
                        var enabledText = "Enabled";
                        SiGui.TextColoured(Colours.Success, enabledText);
                        break;
                    case ModuleState.Loading:
                        var loadingText = "Loading";
                        SiGui.TextColoured(Colours.Warning, loadingText);
                        break;
                    case ModuleState.Disabled:
                        var disabledText = "Disabled";
                        SiGui.TextColoured(ImGuiColors.DalamudGrey, disabledText);
                        break;
                    case ModuleState.Unloading:
                        var unloadingText = "Unloading";
                        SiGui.TextColoured(Colours.Warning, unloadingText);
                        break;
                    case ModuleState.Error:
                        var errorText = "Error";
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
                ImGui.SetCursorPos(new Vector2((ImGui.GetContentRegionAvail().X / 2) - (ImGui.CalcTextSize("Select a module to view its settings.").X / 2), (ImGui.GetContentRegionAvail().Y / 2) - (ImGui.CalcTextSize("Select a module to view its settings.").Y / 2)));
                SiGui.TextDisabledWrapped("Select a module to view its settings.");
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
