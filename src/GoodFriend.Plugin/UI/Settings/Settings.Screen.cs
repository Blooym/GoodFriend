namespace GoodFriend.UI.Settings;

using System;
using System.Numerics;
using System.Linq;
using GoodFriend.Base;
using ImGuiNET;

class SettingsScreen : IDisposable
{
    private bool _visible = false;
    private bool _showAdvanced = false;
    public bool visible { get { return _visible; } set { _visible = value; } }

    /// <summary>
    ///     Instantiates a new settings window.
    /// </summary>
    public SettingsScreen() { }


    /// <summary>
    ///     Disposes of the settings window.
    /// </summary>
    public void Dispose() { }


    /// <summary>
    ///     Draws all UI elements associated with the settings window.
    /// </summary>
    public void Draw() => DrawSettingsWindow();


    /// <summary> 
    ///     Draws the settings window.
    /// </summary>
    private void DrawSettingsWindow()
    {

        if (!this._visible) return;

        var showAdvanced = this._showAdvanced;
        var notificationType = Enum.GetName(typeof(NotificationType), Service.Configuration.NotificationType);
        var APIUrl = Service.Configuration.APIUrl.ToString();
        var loginMessage = Service.Configuration.FriendLoggedInMessage;
        var logoutMessage = Service.Configuration.FriendLoggedOutMessage;
        var eventSecret = Service.Configuration.EventSecret;

        // Draw the settings window.
        if (showAdvanced) ImGui.SetNextWindowSize(new Vector2(500, 260));
        else ImGui.SetNextWindowSize(new Vector2(500, 200));
        if (ImGui.Begin("Good Friend", ref this._visible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse))
        {
            // Notification type dropdown
            if (ImGui.BeginCombo("Notification Type", notificationType))
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
            if (ImGui.InputText("Login Message", ref loginMessage, 64))
            {
                bool error = false;
                try { string.Format(loginMessage, "test"); }
                catch { error = true; }

                if (!error)
                {
                    Service.Configuration.FriendLoggedInMessage = loginMessage;
                    Service.Configuration.Save();
                }
            }

            // Logout message input
            if (ImGui.InputText("Logout Message", ref logoutMessage, 64))
            {
                bool error = false;
                try { string.Format(logoutMessage, "test"); }
                catch { error = true; }

                if (!error)
                {
                    Service.Configuration.FriendLoggedOutMessage = logoutMessage;
                    Service.Configuration.Save();
                }
            }

            ImGui.TextDisabled("Use the {0} placeholder for the friend name.");

            // Advanced settings
            ImGui.NewLine();
            if (ImGui.Checkbox("Show Advanced Settings", ref showAdvanced)) this._showAdvanced = showAdvanced;

            if (this._showAdvanced)
            {
                // API URL input
                if (ImGui.InputText("API URL", ref APIUrl, 64))
                {
                    bool error = false;
                    try { new Uri(APIUrl); }
                    catch { error = true; }

                    if (!error)
                    {
                        Service.Configuration.APIUrl = new Uri(APIUrl);
                        Service.Configuration.Save();
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Default"))
                {
                    Service.Configuration.ResetApiUrl();
                    Service.Configuration.Save();
                }

                // Secret code input
                if (ImGui.InputTextWithHint("Event Secret", "Secret to use for events, none for public.", ref eventSecret, 64))
                {
                    Service.Configuration.EventSecret = eventSecret.Where(char.IsLetterOrDigit).Aggregate("", (current, c) => current + c);
                    Service.Configuration.Save();
                }
            }
        }
    }
}
