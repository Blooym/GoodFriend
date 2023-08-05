using System;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
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
                SiGui.TextWrappedColoured(Colours.Warning, Strings.UI_MainWindow_SettingsScreen_RestartNeeded);
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
            if (SiGui.InputText(Strings.UI_MainWindow_SettingsScreen_Setting_APIURL, ref apiUrl, 250, true))
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
                    ToastHelper.ShowErrorToast(Strings.UI_MainWindow_SettingsScreen_Setting_APIURL_Invalid);
                    SoundEffectHelper.PlaySound(SoundEffect.Se11);
                }
            }
            SiGui.TextDisabledWrapped(Strings.UI_MainWindow_SettingsScreen_Setting_APIURL_Description);
            ImGui.Dummy(Spacing.ReadableSpacing);

            var apiKey = Services.PluginConfiguration.ApiConfig.ApiKey;
            if (SiGui.InputText("API Key", ref apiKey, 250, false))
            {
                if (apiKey != Services.PluginConfiguration.ApiConfig.ApiKey)
                {
                    restartRequired = true;
                    Services.PluginConfiguration.ApiConfig.ApiKey = apiKey;
                    Services.PluginConfiguration.Save();
                }
            }
            SiGui.TextDisabledWrapped("The API key to use for requests that require authentication. Only instance administrators will need to set this.");
        }

        private enum SettingsOption
        {
            API,
        }
    }
}
