using System;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using ImGuiNET;
using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens;

internal static class SettingsScreen
{
    private static SettingsOption CurrentSetting { get; set; } = SettingsOption.Api;
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
            case SettingsOption.Api:
                DrawAPISettings();
                break;
        }
    }

    private static bool revealSensitiveConfig;

    /// <summary>
    ///     Draws the general settings.
    /// </summary>
    private static void DrawAPISettings()
    {
        var apiUrl = Services.PluginConfiguration.ApiConfig.BaseUrl.ToString();
        if (SiGui.InputText(Strings.UI_MainWindow_SettingsScreen_Setting_APIURL, ref apiUrl, 250, true))
        {
            try
            {
                var uri = new Uri(apiUrl);
                if (uri != Services.PluginConfiguration.ApiConfig.BaseUrl)
                {
                    restartRequired = true;
                    Services.PluginConfiguration.ApiConfig.BaseUrl = uri;
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

        if (ImGui.Button(revealSensitiveConfig ? Strings.UI_MainWindow_SettingsScreen_Api_HideSensitive : Strings.UI_MainWindow_SettingsScreen_Api_ShowSensitive))
        {
            revealSensitiveConfig = !revealSensitiveConfig;
        }
        ImGui.Dummy(Spacing.ReadableSpacing);

        if (revealSensitiveConfig)
        {
            var authKey = Services.PluginConfiguration.ApiConfig.AuthKey;
            if (SiGui.InputText(Strings.UI_MainWindow_SettingsScreen_Api_AuthToken, ref authKey, 250, false))
            {
                if (authKey != Services.PluginConfiguration.ApiConfig.AuthKey)
                {
                    restartRequired = true;
                    Services.PluginConfiguration.ApiConfig.AuthKey = authKey;
                    Services.PluginConfiguration.Save();
                }
            }
            SiGui.TextDisabledWrapped(Strings.UI_MainWindow_SettingsScreen_Api_AuthToken_Description);
        }
    }

    private enum SettingsOption
    {
        Api,
    }
}
