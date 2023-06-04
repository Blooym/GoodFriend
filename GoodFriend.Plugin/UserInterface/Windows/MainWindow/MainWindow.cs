using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.UserInterface.Components;
using GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens;
using ImGuiNET;
using Sirensong.UserInterface;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow
{
    internal sealed partial class MainWindow : Window
    {
        /// <summary>
        ///     The width of the sidebar.
        /// </summary>
        private const float SidebarWidthPercentage = 0.25f;

        /// <summary>
        ///     The width of the list.
        /// </summary>
        private const float ListWidthPercentage = 0.75f;

        /// <summary>
        ///     The space left at the bottom of the window wrapper.
        /// </summary>
        private const uint WindowWrapperBottomSpace = 40;

        /// <summary>
        ///     The currently selected tab.
        /// </summary>
        private MainWindowScreen CurrentScreen { get; set; } = MainWindowScreen.Modules;

        /// <inheritdoc />
        public MainWindow() : base(Constants.PluginName)
        {
            this.Size = new Vector2(680, 660);
            this.SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = new Vector2(750, 660),
                MaximumSize = new Vector2(950, 700)
            };
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            SiGui.TextDisabledWrapped(this.CurrentScreen.ToString());
            if (ImGui.BeginChild("MainWindowWrapper", new Vector2(0, ImGui.GetContentRegionAvail().Y - WindowWrapperBottomSpace), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                switch (this.CurrentScreen)
                {
                    case MainWindowScreen.Modules:
                        DrawModules();
                        break;
                    case MainWindowScreen.Settings:
                        DrawSettings();
                        break;
                }
            }
            ImGui.EndChild();

            ButtonRowComponent.DrawRow(new Dictionary<(FontAwesomeIcon, Vector4?, string), Action>
            {
                { (FontAwesomeIcon.Home, null, "Modules"), () => this.CurrentScreen = MainWindowScreen.Modules },
                { (FontAwesomeIcon.Cog, null, "Settings"), () => this.CurrentScreen = MainWindowScreen.Settings },
                { (FontAwesomeIcon.Heart, ImGuiColors.ParsedPurple, "Donate (Opens in browser)"), () => Util.OpenLink(Constants.Link.Donate) },
            });
        }

        /// <summary>
        ///     Draws the module content.
        /// </summary>
        private static void DrawModules()
        => PanelComponent.DrawSplitPanels(ImGui.GetContentRegionAvail().X * SidebarWidthPercentage, ImGui.GetContentRegionAvail().X * ListWidthPercentage,
            ModuleScreen.DrawModuleList,
            ModuleScreen.DrawModuleDetails);

        /// <summary>
        ///     Draws the settings content.
        /// </summary>
        private static void DrawSettings()
            => PanelComponent.DrawSplitPanels(ImGui.GetContentRegionAvail().X * SidebarWidthPercentage, ImGui.GetContentRegionAvail().X * ListWidthPercentage,
                SettingsScreen.DrawSettingsList,
                SettingsScreen.DrawSettingDetails);

        /// <summary>
        ///     The tabs of the main window.
        /// </summary>
        private enum MainWindowScreen
        {
            /// <summary>
            ///     The modules tab.
            /// </summary>
            Modules,

            /// <summary>
            ///     The settings tab.
            /// </summary>
            Settings,
        }
    }
}
