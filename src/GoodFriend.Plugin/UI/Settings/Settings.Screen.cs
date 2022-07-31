namespace GoodFriend.UI.Settings;

using System;
using System.Numerics;
using System.Linq;
using System.IO;
using GoodFriend.Base;
using Dalamud.Logging;
using ImGuiNET;
using CheapLoc;

sealed internal class SettingsScreen : IDisposable
{
    /// <summary>
    ///     Disposes of the settings window.
    /// </summary>
    public void Dispose() { }

    /// <summary>
    ///     Draws all UI elements associated with the settings window.
    /// </summary>
    public void Draw() => DrawSettingsWindow();

    private bool _visible = false;
    public bool visible { get { return _visible; } set { _visible = value; } }
    private bool _showAdvanced = false;

    /// <summary> 
    ///     Draws the settings window.
    /// </summary>
    private void DrawSettingsWindow()
    {

        if (!this._visible) return;

        var showAdvanced = this._showAdvanced;
        var notificationType = Enum.GetName(typeof(NotificationType), Service.Configuration.NotificationType);
        var loginMessage = Service.Configuration.FriendLoggedInMessage;
        var logoutMessage = Service.Configuration.FriendLoggedOutMessage;
        var hideSameFC = Service.Configuration.HideSameFC;

        // Draw the settings window.
        if (showAdvanced) ImGui.SetNextWindowSize(new Vector2(500, 280));
        else ImGui.SetNextWindowSize(new Vector2(500, 200));
        if (ImGui.Begin(PStrings.pluginName, ref this._visible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
        {
            // Hide FC Members dropdown.
            if (ImGui.BeginCombo(Loc.Localize("UI.Settings.HideSameFC", "Hide FC Members"), hideSameFC.ToString()))
            {
                if (ImGui.Selectable(Loc.Localize("UI.Settings.HideSameFC.True", "True"), hideSameFC))
                {
                    Service.Configuration.HideSameFC = true;
                }
                if (ImGui.Selectable(Loc.Localize("UI.Settings.HideSameFC.False", "False"), !hideSameFC))
                {
                    Service.Configuration.HideSameFC = false;
                }

                Service.Configuration.Save();
                ImGui.EndCombo();
            }


            // Notification type dropdown
            if (ImGui.BeginCombo(Loc.Localize("UI.Settings.NotificationType", "Notification Type"), notificationType))
            {
                foreach (var notification in Enum.GetNames(typeof(NotificationType)))
                {
                    if (ImGui.Selectable(notification, notification == notificationType))
                    {
                        Service.Configuration.NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notification);
                        Service.Configuration.Save();
                    }
                }
                ImGui.EndCombo();
            }


            // Login message input
            if (ImGui.InputText(Loc.Localize("UI.Settings.LoginMessage", "Login Message"), ref loginMessage, 64))
            {
                bool error = false;
                try { string.Format(loginMessage, "test"); }
                catch { error = true; }

                if (!error && loginMessage.Contains("{0}"))
                {
                    Service.Configuration.FriendLoggedInMessage = loginMessage.Trim();
                    Service.Configuration.Save();
                }
            }


            // Logout message input
            if (ImGui.InputText(Loc.Localize("UI.Settings.LogoutMessage", "Logout Message"), ref logoutMessage, 64))
            {
                bool error = false;
                try { string.Format(logoutMessage, "test"); }
                catch { error = true; }

                if (!error && logoutMessage.Contains("{0}"))
                {
                    Service.Configuration.FriendLoggedOutMessage = logoutMessage.Trim();
                    Service.Configuration.Save();
                }
            }


            // Advanced settings
            ImGui.NewLine();
            if (ImGui.Checkbox(Loc.Localize("UI.Settings.ShowAdvanced", "Show Advanced Settings"), ref showAdvanced)) this._showAdvanced = showAdvanced;

            if (this._showAdvanced)
            {

                var APIUrl = Service.Configuration.APIUrl.ToString();
                var friendshipCode = Service.Configuration.FriendshipCode;

                // Secret code input
                if (ImGui.InputTextWithHint(Loc.Localize("UI.Settings.FriendshipCode", "Friendship Code"), Loc.Localize("UI.Settings.FriendshipCode.Hint", "Leave blank to get/recieve notifications from all friends"), ref friendshipCode, 64))
                {
                    Service.Configuration.FriendshipCode = friendshipCode.Where(char.IsLetterOrDigit).Aggregate("", (current, c) => current + c);
                    Service.Configuration.Save();
                }


                // API URL input
                if (ImGui.InputText(Loc.Localize("UI.Settings.APIURL", "API URL"), ref APIUrl, 64))
                {
                    bool error = false;
                    try { new Uri(APIUrl); }
                    catch { error = true; }

                    if (!error)
                    {
                        Service.Configuration.APIUrl = new Uri(APIUrl);
                        Service.Configuration.Save();
                    }
                    else
                    {
                        Service.Configuration.ResetApiUrl();
                        Service.Configuration.Save();
                    }
                }

#if DEBUG
                var localizableOutputDir = Service.Configuration.localizableOutputDir;

                if (ImGui.InputText("Localizable Dir", ref localizableOutputDir, 1000))
                {
                    Service.Configuration.localizableOutputDir = localizableOutputDir;
                    Service.Configuration.Save();
                }

                ImGui.SameLine();

                if (ImGui.Button("Export"))
                {
                    try
                    {
                        var directory = Directory.GetCurrentDirectory();
                        Directory.SetCurrentDirectory(localizableOutputDir);
                        Loc.ExportLocalizable();
                        File.Copy(Path.Combine(localizableOutputDir, "GoodFriend_Localizable.json"), Path.Combine(localizableOutputDir, "en.json"), true);
                        Directory.SetCurrentDirectory(directory);
                        Service.PluginInterface.UiBuilder.AddNotification("Localization exported successfully.", "KikoGuide", Dalamud.Interface.Internal.Notifications.NotificationType.Success);
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error($"Failed to export localization {e.Message}");
                        Service.PluginInterface.UiBuilder.AddNotification("Something went wrong exporting, see /xllog for details.", "KikoGuide", Dalamud.Interface.Internal.Notifications.NotificationType.Success);
                    }
                }
#endif
            }
        }
    }
}
