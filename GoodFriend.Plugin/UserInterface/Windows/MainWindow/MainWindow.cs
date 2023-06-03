using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;
using Sirensong.UserInterface.Windowing;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow
{
    internal sealed class MainWindow : Window
    {
        /// <inheritdoc />
        public MainWindow() : base("Settings")
        {
            this.Size = new Vector2(700, 450);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.Flags = ImGuiWindowFlagExtra.NoScroll;
        }

        private ApiModuleBase? SelectedModule { get; set; }

        /// <inheritdoc />
        public override void Draw()
        {
            var selectedModule = this.SelectedModule;
            if (ImGui.BeginTable("PluginSettings", 2, ImGuiTableFlags.BordersInnerV))
            {
                ImGui.TableSetupColumn("PluginSettingsSidebar", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X * 0.25f);
                ImGui.TableSetupColumn("PluginSettingsList", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X * 0.75f);
                ImGui.TableNextRow();

                // Sidebar
                ImGui.TableNextColumn();
                if (ImGui.BeginChild("PluginSettingsSidebarChild"))
                {
                    // Display modules under their appropriate tag
                    ApiModuleTag? lastTag = null;
                    foreach (var module in Services.ApiModuleService.GetModules().OrderBy(x => x.Tag))
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

                        if (ImGui.Selectable(module.Name, selectedModule == module))
                        {
                            this.SelectedModule = module;
                        }
                    }
                }
                ImGui.EndChild();

                if (selectedModule == null)
                {
                    ImGui.EndTable();
                    return;
                }

                // Listings
                ImGui.TableNextColumn();
                if (ImGui.BeginChild("PluginSettingsListChild"))
                {
                    SiGui.TextDisabledWrapped(selectedModule.Name);
                    ImGui.SameLine();
                    switch (selectedModule.State)
                    {
                        case ApiModuleState.Enabled:
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Enabled").X - 10);
                            SiGui.TextColoured(Colours.Success, "Enabled");
                            break;
                        case ApiModuleState.Loading:
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Loading").X - 10);
                            SiGui.TextColoured(Colours.Warning, "Loading");
                            break;
                        case ApiModuleState.Disabled:
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Disabled").X - 10);
                            SiGui.TextColoured(ImGuiColors.DalamudGrey, "Disabled");
                            break;
                        case ApiModuleState.Unloading:
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Unloading").X - 10);
                            SiGui.TextColoured(Colours.Warning, "Unloading");
                            break;
                        case ApiModuleState.Error:
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Error").X - 10);
                            SiGui.TextColoured(Colours.Error, "Error");
                            break;
                    }
                    ImGui.Separator();
                    ImGui.Dummy(Spacing.HeaderSpacing);

                    if (ImGui.BeginChild("PluginSettingsListChildScrolling"))
                    {
                        selectedModule?.Draw();
                    }
                    ImGui.EndChild();
                }
                ImGui.EndChild();

                ImGui.EndTable();
            }
        }
    }
}
