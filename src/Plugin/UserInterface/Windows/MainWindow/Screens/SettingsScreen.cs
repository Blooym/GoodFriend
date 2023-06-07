using System;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens
{
    internal static class SettingsScreen
    {
        private static SettingsOption CurrentSetting { get; set; } = SettingsOption.API;
        private static bool restartRequired;

        /// <summary>
        ///     Draws the settings list.
        /// </summary>
        public static void DrawSettingsList()
        {
            foreach (var setting in Enum.GetValues<SettingsOption>())
            {
                if (ImGui.Selectable(setting.ToString(), setting == CurrentSetting))
                {
                    CurrentSetting = setting;
                }
            }
        }

        /// <summary>
        ///     Draws the settings menu.
        /// </summary>
        public static void DrawSettingDetails()
        {
            if (restartRequired)
            {
                SiGui.TextWrappedColoured(Colours.Warning, "One or more changes may require a plugin restart to take effect.");
                ImGui.Dummy(Spacing.ReadableSpacing);
            }

            SiGui.Heading(CurrentSetting.ToString());
            switch (CurrentSetting)
            {
                case SettingsOption.API:
                    DrawAPISettings();
                    break;
            }
        }

        /// <summary>
        ///     Draws the general settings.
        /// </summary>
        private static void DrawAPISettings()
        {
            var apiUrl = Services.PluginConfiguration.ApiConfig.ApiBaseUrl.ToString();
            if (SiGui.InputText("API URL", ref apiUrl, 250, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                try
                {
                    var uri = new Uri(apiUrl);
                    if (uri != Services.PluginConfiguration.ApiConfig.ApiBaseUrl)
                    {
                        restartRequired = true;
                        Services.PluginConfiguration.ApiConfig.ApiBaseUrl = uri;
                        Services.PluginConfiguration.Save();
                    }
                }
                catch (UriFormatException)
                {
                    ToastHelper.ShowErrorToast("Please enter a valid URL.");
                    SoundEffectHelper.PlaySound(SoundEffect.Se11);
                }
            }
            SiGui.TextDisabledWrapped("This is the URL that the plugin will communicate with for API requests. Most users will not need to change this.");
        }

        private enum SettingsOption
        {
            API,
        }
    }
}
