using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using GoodFriend.Plugin.Api.Modules.Required;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens
{
    internal static class ModuleScreen
    {
        /// <summary>
        ///     The current module.
        /// </summary>
        private static ApiModuleBase? CurrentModule { get; set; } = Services.ApiModuleService.GetModule<InstanceInfoModule>();

        /// <summary>
        ///     Draws the list panel of the main window.
        /// </summary>
        public static void DrawModuleList()
        {
            ApiModuleTag? lastTag = null;
            foreach (var module in Services.ApiModuleService.GetModules().OrderBy(x => x.Tag).OrderByDescending(x => x.DisplayWeight))
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

                if (ImGui.Selectable(module.Name, CurrentModule == module))
                {
                    CurrentModule = module;
                }

                if (module.GetType().BaseType == typeof(ApiRequiredModule))
                {
                    continue;
                }

                switch (module.State)
                {
                    case ApiModuleState.Enabled:
                        var enabledText = "Enabled";
                        SiGui.TextColoured(Colours.Success, enabledText);
                        break;
                    case ApiModuleState.Loading:
                        var loadingText = "Loading";
                        SiGui.TextColoured(Colours.Warning, loadingText);
                        break;
                    case ApiModuleState.Disabled:
                        var disabledText = "Disabled";
                        SiGui.TextColoured(ImGuiColors.DalamudGrey, disabledText);
                        break;
                    case ApiModuleState.Unloading:
                        var unloadingText = "Unloading";
                        SiGui.TextColoured(Colours.Warning, unloadingText);
                        break;
                    case ApiModuleState.Error:
                        var errorText = "Error";
                        SiGui.TextColoured(Colours.Error, errorText);
                        break;
                }

                ImGui.Dummy(Spacing.ReadableSpacing);
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
            if (ImGui.BeginChild("PluginSettingsListChildScrolling"))
            {
                CurrentModule.Draw();
            }
            ImGui.EndChild();
        }
    }
}