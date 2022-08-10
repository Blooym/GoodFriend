namespace GoodFriend.UI.Screens.Settings;

using System;
using System.Reflection;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface;
using ImGuiNET;
using GoodFriend.Base;
using GoodFriend.Utils;
using GoodFriend.Interfaces;
using GoodFriend.UI.Components;

sealed public class SettingsScreen : IScreen
{

    public SettingsPresenter presenter = new SettingsPresenter();

    public void Draw() => DrawSettingsWindow();
    public void Show() => presenter.isVisible = true;
    public void Hide() => presenter.isVisible = false;
    public void Dispose() => this.presenter.Dispose();


    /// <summary> Should advanced settings be drawn? </summary>
    private bool _showAdvanced = false;

    /// <summary> 
    ///     Draws the settings window.
    /// </summary>
    private void DrawSettingsWindow()
    {
        if (!presenter.isVisible) return;

        ImGui.SetNextWindowSize(new Vector2(600, 320), ImGuiCond.Appearing);
        if (ImGui.Begin(PStrings.pluginName, ref presenter.isVisible, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
        {

            this.DrawTopBar();
            this.DrawNormSettings();
            this.DrawAdvancedSettings();
        }
    }


    /// <summary> Draws the buttons for the settings window. </summary>
    private void DrawTopBar()
    {

#if DEBUG
        this.presenter.dialogManager.Draw();
        if (ImGui.Button("Export Localizable")) this.presenter.dialogManager.OpenFolderDialog("Select Export Directory", this.presenter.OnDirectoryPicked);
#endif

        // Top bar of settings.
        ImGui.TextWrapped($"{PStrings.pluginName} ver.{Assembly.GetExecutingAssembly().GetName().Version} | {TStrings.SettingsAPIConnected(PluginService.APIClient.IsConnected)}");
        ImGui.SameLine();

        // Reconnect button
        ImGui.BeginDisabled(presenter.reconnectCooldownActive | PluginService.APIClient.IsConnected | PluginService.ClientState.LocalPlayer == null);
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plug)) presenter.ReconnectWithCooldown();
        Tooltips.AddTooltip(TStrings.SettingsAPIReconnect());
        ImGui.EndDisabled();
        ImGui.SameLine();


        // Support button
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Heart)) Common.OpenLink(PStrings.supportButtonUrl);
        Tooltips.AddTooltip(TStrings.SupportText());
        ImGui.Separator();
    }


    /// <summary> Draws the normal settings </summary>
    private void DrawNormSettings()
    {
        var showAdvanced = this._showAdvanced;
        var notificationType = Enum.GetName(typeof(NotificationType), PluginService.Configuration.NotificationType);
        var loginMessage = PluginService.Configuration.FriendLoggedInMessage;
        var logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
        var hideSameFC = PluginService.Configuration.HideSameFC;
        var friendshipCode = PluginService.Configuration.FriendshipCode;

        // Hide FC Members dropdown.
        if (ImGui.BeginCombo(TStrings.SettingsHideSameFC(), hideSameFC ? TStrings.SettingsHideSameFCEnabled() : TStrings.SettingsHideSameFCDisabled()))
        {
            if (ImGui.Selectable(TStrings.SettingsHideSameFCEnabled(), hideSameFC))
            {
                PluginService.Configuration.HideSameFC = true;
            }
            if (ImGui.Selectable(TStrings.SettingsHideSameFCDisabled(), !hideSameFC))
            {
                PluginService.Configuration.HideSameFC = false;
            }

            PluginService.Configuration.Save();
            ImGui.EndCombo();
        }
        Tooltips.Questionmark(TStrings.SettingsHideSameTCTooltip());


        // Notification type dropdown
        if (ImGui.BeginCombo(TStrings.SettingsNotificationType(), notificationType))
        {
            foreach (var notification in Enum.GetNames(typeof(NotificationType)))
            {
                if (ImGui.Selectable(notification, notification == notificationType))
                {
                    PluginService.Configuration.NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notification);
                    PluginService.Configuration.Save();
                }
            }
            ImGui.EndCombo();
        }
        Tooltips.Questionmark(TStrings.SettingsNotificationTypeTooltip());


        // Login message input
        if (ImGui.InputText(TStrings.SettingsLoginMessage(), ref loginMessage, 64))
        {
            bool error = false;
            try { string.Format(loginMessage, "test"); }
            catch { error = true; }

            if (!error && loginMessage.Contains("{0}"))
            {
                PluginService.Configuration.FriendLoggedInMessage = loginMessage.Trim();
                PluginService.Configuration.Save();
            }
        }
        Tooltips.Questionmark(TStrings.SettingsLoginMessageTooltip());


        // Logout message input
        if (ImGui.InputText(TStrings.SettingsLogoutMessage(), ref logoutMessage, 64))
        {
            bool error = false;
            try { string.Format(logoutMessage, "test"); }
            catch { error = true; }

            if (!error && logoutMessage.Contains("{0}"))
            {
                PluginService.Configuration.FriendLoggedOutMessage = logoutMessage.Trim();
                PluginService.Configuration.Save();
            }
        }
        Tooltips.Questionmark(TStrings.SettingsLogoutMessageTooltip());


        // Secret code input
        if (ImGui.InputTextWithHint(TStrings.SettingsFriendshipCode(), TStrings.SettingsFriendshipCodeHint(), ref friendshipCode, 64))
        {
            PluginService.Configuration.FriendshipCode = friendshipCode;
            PluginService.Configuration.Save();
        }
        Tooltips.Questionmark(TStrings.SettingsFriendshipCodeHint());


        // Advanced settings
        ImGui.NewLine();
        if (ImGui.Checkbox(TStrings.SettingsShowAdvanced(), ref showAdvanced)) this._showAdvanced = showAdvanced;
    }


    /// <summary> Draws the advanced settings </summary>
    private void DrawAdvancedSettings()
    {
        if (!this._showAdvanced) return;

        var APIUrl = PluginService.Configuration.APIUrl.ToString();
        var saltMethod = PluginService.Configuration.SaltMethod;

        // Salt Mode dropdown
        if (ImGui.BeginCombo(TStrings.SettingsSaltMode(), saltMethod.ToString()))
        {
            if (ImGui.Selectable(TStrings.SettingsSaltModeStrict(), saltMethod == SaltMethods.Strict))
            {
                PluginService.Configuration.SaltMethod = SaltMethods.Strict;
            }
            if (ImGui.Selectable(TStrings.SettingsSaltModeRelaxed(), saltMethod == SaltMethods.Relaxed))
            {
                PluginService.Configuration.SaltMethod = SaltMethods.Relaxed;
            }

            PluginService.Configuration.Save();
            ImGui.EndCombo();
        }
        if (saltMethod == SaltMethods.Relaxed) Tooltips.Warning(TStrings.SettingsSaltModeWarning());
        else Tooltips.Questionmark(TStrings.SettingsSaltModeTooltip());


        // API URL input
        if (ImGui.InputText(TStrings.SettingsAPIURL(), ref APIUrl, 64))
        {
            bool error = false;
            try { new Uri(APIUrl); }
            catch { error = true; }

            if (!error)
            {
                PluginService.Configuration.APIUrl = new Uri(APIUrl);
                PluginService.Configuration.Save();
            }
            else PluginService.Configuration.ResetApiUrl();
        }
        if (!APIUrl.StartsWith("https")) Tooltips.Warning(TStrings.SettingsAPINotHttps());
        else Tooltips.Questionmark(TStrings.SettingsAPIURLTooltip());
    }
}
