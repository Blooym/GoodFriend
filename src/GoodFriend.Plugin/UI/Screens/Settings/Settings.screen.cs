namespace GoodFriend.UI.Screens.Settings;

using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using CheapLoc;
using GoodFriend.Base;
using GoodFriend.UI.Components;
using GoodFriend.Utils;

sealed public class SettingsScreen : IDisposable
{

    public SettingsPresenter presenter = new SettingsPresenter();

    public void Draw() => DrawSettingsWindow();
    public void Dispose() => this.presenter.Dispose();
    public void Show() => this.presenter.isVisible = true;
    public void Hide() => this.presenter.isVisible = false;

    private bool _showAdvanced = false;

    /// <summary> 
    ///     Draws the settings window.
    /// </summary>
    private void DrawSettingsWindow()
    {

        if (!presenter.isVisible) return;

        var showAdvanced = this._showAdvanced;
        var notificationType = Enum.GetName(typeof(NotificationType), PluginService.Configuration.NotificationType);
        var loginMessage = PluginService.Configuration.FriendLoggedInMessage;
        var logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
        var hideSameFC = PluginService.Configuration.HideSameFC;
        var friendshipCode = PluginService.Configuration.FriendshipCode;

        // Draw the settings window.
        if (showAdvanced) ImGui.SetNextWindowSize(new Vector2(580, 320));
        else ImGui.SetNextWindowSize(new Vector2(580, 250));
        if (ImGui.Begin(PStrings.pluginName, ref presenter.isVisible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
        {
            ImGui.TextDisabled($"{PStrings.pluginName} v{Assembly.GetExecutingAssembly().GetName().Version} | API Connected: {PluginService.APIClient.IsConnected}");

            // Hide FC Members dropdown.
            if (ImGui.BeginCombo(Loc.Localize("UI.Settings.HideSameFC", "Hide FC Members"), hideSameFC.ToString()))
            {
                if (ImGui.Selectable(Loc.Localize("UI.Settings.HideSameFC.Enabled", "Enabled"), hideSameFC))
                {
                    PluginService.Configuration.HideSameFC = true;
                }
                if (ImGui.Selectable(Loc.Localize("UI.Settings.HideSameFC.Disabled", "Disabled"), !hideSameFC))
                {
                    PluginService.Configuration.HideSameFC = false;
                }

                PluginService.Configuration.Save();
                ImGui.EndCombo();
            }
            Tooltips.Questionmark("Should the plugin display notifications for members of your free company that you have added?");


            // Notification type dropdown
            if (ImGui.BeginCombo(Loc.Localize("UI.Settings.NotificationType", "Notification Type"), notificationType))
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
            Tooltips.Questionmark("The place to display notifications when a friend logs in or logs out.");


            // Login message input
            if (ImGui.InputText(Loc.Localize("UI.Settings.LoginMessage", "Login Message"), ref loginMessage, 64))
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
            Tooltips.Questionmark("The message to show when a friend logs in out.\n\n{0}: Name of the friend.");


            // Logout message input
            if (ImGui.InputText(Loc.Localize("UI.Settings.LogoutMessage", "Logout Message"), ref logoutMessage, 64))
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
            Tooltips.Questionmark("The message to show when a friend logs out.\n\n{0}: Name of the friend.");

            // Secret code input
            if (ImGui.InputTextWithHint(Loc.Localize("UI.Settings.FriendshipCode", "Friendship Code"), Loc.Localize("UI.Settings.FriendshipCode.Hint", "Leave blank to get/recieve notifications from all friends"), ref friendshipCode, 64))
            {
                PluginService.Configuration.FriendshipCode = friendshipCode.Where(char.IsLetterOrDigit).Aggregate("", (current, c) => current + c);
                PluginService.Configuration.Save();
            }
            Tooltips.Questionmark("Your friend code determines which friends you send and recieve notifications from.\nOnly friends with matching codes will get notified of eachother");


            // Advanced settings
            ImGui.NewLine();
            if (ImGui.Checkbox(Loc.Localize("UI.Settings.ShowAdvanced", "Show Advanced Settings"), ref showAdvanced)) this._showAdvanced = showAdvanced;

            if (this._showAdvanced)
            {

                var APIUrl = PluginService.Configuration.APIUrl.ToString();
                var saltMethod = PluginService.Configuration.SaltMethod;

                // Salt Mode dropdown
                if (ImGui.BeginCombo(Loc.Localize("UI.Settings.SaltMode", "Salt Mode"), saltMethod.ToString()))
                {
                    if (ImGui.Selectable(Loc.Localize("UI.Settings.SaltMode.Strict", "Strict"), saltMethod == SaltMethods.Strict))
                    {
                        PluginService.Configuration.SaltMethod = SaltMethods.Strict;
                    }
                    if (ImGui.Selectable(Loc.Localize("UI.Settings.SaltMode.Relaxed", "Relaxed"), saltMethod == SaltMethods.Relaxed))
                    {
                        PluginService.Configuration.SaltMethod = SaltMethods.Relaxed;
                    }

                    PluginService.Configuration.Save();
                    ImGui.EndCombo();
                }
                if (saltMethod == SaltMethods.Relaxed) Tooltips.Warning("Relaxed salt mode is not recommended unless you are trying to interact with other plugins using the same API.\n\nYou will not recieve notifications from Strict users");
                else Tooltips.Questionmark("Strict: Validation is done using both Friend Code & Plugin Assembly\nRelaxed: Validation is done using Friend Code\n\nKeep this on strict if you are not having any issues");


                // API URL input
                if (ImGui.InputText(Loc.Localize("UI.Settings.APIURL", "API URL"), ref APIUrl, 64))
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
                if (!APIUrl.StartsWith("https")) Tooltips.Warning("Warning: You are not using HTTPs for the API! This is VERY insecure and could lead to major security problems.");
                else Tooltips.Questionmark("The API is used to send and recieve notifications from friends using the same API & Code.\n\nYou should not change this unless you know what you are doing.");

#if DEBUG
                this.presenter.dialogManager.Draw();
                if (ImGui.Button("Export Localizable")) this.presenter.dialogManager.OpenFolderDialog("Select Export Directory", this.presenter.OnDirectoryPicked);
#endif

            }
        }
    }
}
