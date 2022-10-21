namespace GoodFriend.UI.Windows.Main
{
    using System;
    using System.Numerics;
    using System.Linq;
    using GoodFriend.Base;
    using GoodFriend.Enums;
    using GoodFriend.Utils;
    using GoodFriend.Managers;
    using GoodFriend.UI.Components;
    using Dalamud.Utility;
    using Dalamud.Interface.Windowing;
    using ImGuiNET;

    /// <summary>
    ///     The settings window for the plugin.
    /// </summary>
    public sealed class MainWindow : Window, IDisposable
    {
        /// <summary>
        ///     The presenter associated with the window, handles all logic and data.
        /// </summary>
        public MainPresenter presenter;

        /// <summary>
        ///     Instantiate a new settings window.
        /// </summary>
        public MainWindow() : base(PStrings.pluginName)
        {
            Flags |= ImGuiWindowFlags.NoScrollbar;
            Flags |= ImGuiWindowFlags.NoResize;
            Flags |= ImGuiWindowFlags.NoDocking;
            presenter = new MainPresenter();
        }

        /// <summary>
        ///     Draws all elements and components for the settings window, including sub-components.
        /// </summary>
#pragma warning disable CS1717
        public override void Draw()
        {
            var statusPageUrl = PluginService.APIClientManager.GetMetadata()?.statusPageUrl;
            var newApiUrl = PluginService.APIClientManager.GetMetadata()?.newApiUrl;
            Components.ConnectionStatusComponent.Draw("ConnectionPageStatusComponent", new Vector2(0, 70 * ImGui.GetIO().FontGlobalScale));

            // Status button
            ImGui.BeginDisabled(statusPageUrl == null);
            if (Tooltips.TooltipButton(TStrings.WindowDropdownOptionsStatus, TStrings.WindowDropdownOptionsStatusTooltip(statusPageUrl), new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
            {
#pragma warning disable CS8604
                Util.OpenLink(statusPageUrl);
#pragma warning restore CS8604
            }
            ImGui.EndDisabled();
            ImGui.SameLine();

            // Donate button
            if (Tooltips.TooltipButton(TStrings.WindowDropdownOptionsSupport, TStrings.WindowDropdownOptionsSupportTooltip, new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Donate);
            ImGui.SameLine();

            // Event log button
            if (Tooltips.TooltipButton(TStrings.WindowDropdownOptionsEventLog, TStrings.WindowDropdownOptionsEventLogTooltip, new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Logs);
            ImGui.SameLine();

            // Settings button
            if (Tooltips.TooltipButton(TStrings.WindowDropdownOptionsSettings, TStrings.WindowDropdownOptionsSettingsTooltip, new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Settings);
            ImGui.Dummy(new Vector2(0, 10));

            // Handle the option dropdowns - size is handled outside of the sub-components for clarity.
            switch (this.presenter.visibleDropdown)
            {
                case MainPresenter.VisibleDropdown.Settings:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 320 * ImGui.GetIO().FontGlobalScale));
                    this._drawSettings();
                    break;
                case MainPresenter.VisibleDropdown.Donate:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 400 * ImGui.GetIO().FontGlobalScale));
                    this._drawSupport();
                    break;
                case MainPresenter.VisibleDropdown.Logs:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 400 * ImGui.GetIO().FontGlobalScale));
                    this._drawLogs();
                    break;
                case MainPresenter.VisibleDropdown.None:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 130 * ImGui.GetIO().FontGlobalScale));
                    break;
            }
        }

        /// <summary>
        ///     Dispose of the window and all associated resources.
        /// </summary>
        public void Dispose() => this.presenter.Dispose();

        /// <summary>
        ///     Draw the settings dropdown.
        /// </summary>
        private unsafe void _drawSettings()
        {
            ImGui.TextDisabled("Settings");
            ImGui.BeginChild("SettingsDropdown");

            try
            {
                if (ImGui.BeginTabBar("SettingsDropdownTabBar"))
                {
                    // "Basic" Settings
                    if (ImGui.BeginTabItem("General"))
                    {
                        var notificationType = Enum.GetName(typeof(NotificationType), PluginService.Configuration.NotificationType);
                        var loginMessage = PluginService.Configuration.FriendLoggedInMessage;
                        var logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
                        var hideSameFC = PluginService.Configuration.HideSameFC;
                        var friendshipCode = PluginService.Configuration.FriendshipCode;

                        ImGui.BeginChild("GeneralSettings");
                        ImGui.BeginTable("SettingsTable", 2);
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableNextRow();

                        // Ignore FC Members
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsIgnoreFC);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##IgnoreFCMembers", hideSameFC ? TStrings.WindowDropdownSettingsEnabled : TStrings.WindowDropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(TStrings.WindowDropdownSettingsEnabled, hideSameFC))
                                PluginService.Configuration.HideSameFC = true;
                            if (ImGui.Selectable(TStrings.WindowDropdownSettingsDisabled, !hideSameFC))
                                PluginService.Configuration.HideSameFC = false;
                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsIgnoreFCTooltip);
                        ImGui.TableNextRow();


                        // Notification Type
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsNotificationType);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##NotificationType", notificationType))
                        {
                            foreach (var type in Enum.GetNames(typeof(NotificationType)))
                            {
                                if (ImGui.Selectable(type, type == notificationType))
                                {
                                    PluginService.Configuration.NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), type);
                                    PluginService.Configuration.Save();
                                }
                            }
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsNotificationTypeTooltip);
                        ImGui.TableNextRow();


                        // Login message
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsLoginMessage);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##LoginMessage", ref loginMessage, 255))
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
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsLoginMessageTooltip);
                        ImGui.TableNextRow();

                        // Logout message
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsLogoutMessage);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##LogoutMessage", ref logoutMessage, 255))
                        {
                            bool error = false;
                            try { string.Format(loginMessage, "test"); }
                            catch { error = true; }

                            if (!error && loginMessage.Contains("{0}"))
                            {
                                PluginService.Configuration.FriendLoggedOutMessage = loginMessage.Trim();
                                PluginService.Configuration.Save();
                            }
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsLogoutMessageTooltip);

                        ImGui.TableNextRow();
                        ImGui.EndTable();
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    // "Advanced" Settings
                    if (ImGui.BeginTabItem("Advanced"))
                    {
                        var showAPIEvents = PluginService.Configuration.ShowAPIEvents;
                        var APIUrl = PluginService.Configuration.APIUrl.ToString();
                        var friendshipCode = PluginService.Configuration.FriendshipCode;
                        var apiToken = PluginService.Configuration.APIBearerToken;
                        var saltMethod = PluginService.Configuration.SaltMethod;

                        ImGui.BeginChild("AdvancedSettings");
                        ImGui.BeginTable("SettingsTable", 2);
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableNextRow();

                        // API Connection Notifications
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsAPINotifications);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##APIConnectionNotifications", showAPIEvents ? TStrings.WindowDropdownSettingsEnabled : TStrings.WindowDropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(TStrings.WindowDropdownSettingsEnabled, showAPIEvents))
                                PluginService.Configuration.ShowAPIEvents = true;
                            if (ImGui.Selectable(TStrings.WindowDropdownSettingsDisabled, !showAPIEvents))
                                PluginService.Configuration.ShowAPIEvents = false;
                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsAPINotificationsTooltip);
                        ImGui.TableNextRow();


                        // Friendship code
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsFriendshipCode);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##FriendshipCode", ref friendshipCode, 255))
                        {
                            PluginService.Configuration.FriendshipCode = friendshipCode;
                            PluginService.Configuration.Save();
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsFriendshipCodeTooltip);
                        ImGui.TableNextRow();


                        // API URL
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsAPIUrl);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##APIUrl", ref APIUrl, 255))
                        {
                            if (APIUrl == PluginService.Configuration.APIUrl.ToString()) return;

                            bool error = false;
                            try { new Uri(APIUrl); }
                            catch { error = true; }

                            if (!error) { PluginService.Configuration.APIUrl = new Uri(APIUrl); PluginService.Configuration.Save(); }
                            else { PluginService.Configuration.APIUrl = PStrings.defaultAPIUrl; PluginService.Configuration.Save(); }
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsAPIUrlTooltip);
                        ImGui.TableNextRow();


                        // API Token
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsAPIToken);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##APIToken", ref apiToken, 255))
                        {
                            PluginService.Configuration.APIBearerToken = apiToken;
                            PluginService.Configuration.Save();
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsAPITokenTooltip);
                        ImGui.TableNextRow();


                        // Salt Method
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(TStrings.WindowDropdownSettingsSaltMethod);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##SaltMethod", saltMethod.ToString()))
                        {
                            foreach (var type in Enum.GetNames(typeof(SaltMethods)))
                            {
                                if (ImGui.Selectable(type, type == saltMethod.ToString()))
                                {
                                    PluginService.Configuration.SaltMethod = (SaltMethods)Enum.Parse(typeof(SaltMethods), type);
                                    PluginService.Configuration.Save();
                                }
                            }
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(TStrings.WindowDropdownSettingsSaltMethodTooltip);

                        ImGui.EndTable();
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

#if DEBUG
                    // "Debug" Settings
                    if (ImGui.BeginTabItem("Developer"))
                    {
                        ImGui.BeginChild("DeveloperSettings");

                        this.presenter.dialogManager.Draw();
                        if (ImGui.Button("Export Localizable", new Vector2(ImGui.GetWindowWidth() - 20, 0)))
                        {
                            this.presenter.dialogManager.OpenFolderDialog("Export Localizable", this.presenter.OnDirectoryPicked);
                        }

                        ImGui.TextDisabled("Detected Friends - Click to copy ContentID hash");
                        ImGui.BeginChild("##Friends");
                        foreach (var friend in FriendList.Get())
                            if (ImGui.Selectable(friend->Name.ToString()))
                            {
                                ImGui.SetClipboardText(Hashing.HashSHA512(friend->ContentId.ToString()));
                                Notifications.Show($"Copied {friend->Name.ToString()}'s ContentID hash to clipboard", NotificationType.Toast);
                            }
                        ImGui.EndChild();

                        ImGui.EndChild();
                    }
#endif
                    ImGui.EndTabBar();
                }
            }
            catch { /* Prevent ImGui errors when the window is smaller than the table */ }
            ImGui.EndChild();
        }

        /// <summary>
        ///     Draw the support dropdown.
        /// </summary>
        private void _drawSupport()
        {
            // Top caption and flavour text.
            ImGui.BeginChild("SupportDropdown");
            ImGui.TextDisabled(TStrings.WindowDropdownSupportTitle);
            ImGui.Separator();
            ImGui.TextWrapped(TStrings.WindowDropdownSupportFlavourText);
            ImGui.Dummy(new Vector2(0, 10));


            // Support the developer button
            ImGui.TextDisabled(TStrings.WindowDropdownSupportDeveloper);
            ImGui.TextWrapped(TStrings.WindowDropdownSupportDeveloperDescription);
            if (Tooltips.TooltipButton(TStrings.WindowDropdownSupportDonate, PStrings.supportButtonUrl))
            {
                Util.OpenLink(PStrings.supportButtonUrl);
            }
            ImGui.Dummy(new Vector2(0, 5));


            var donationPageUrl = PluginService.APIClientManager.GetMetadata()?.donationPageUrl;
            ImGui.TextDisabled(TStrings.WindowDropdownSupportAPIHost);
            ImGui.TextWrapped(TStrings.WindowDropdownSupportAPIHostDescription);
            if (donationPageUrl != null)
            {
                try
                {
                    var uri = new Uri(donationPageUrl);
                    if (Tooltips.TooltipButton(TStrings.WindowDropdownSupportDonate, donationPageUrl))
                    {
                        Util.OpenLink(donationPageUrl);
                    }
                }
                catch
                {
                    ImGui.TextWrapped(TStrings.WindowDropdownSupportAPIHostInvalidUri);
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button(TStrings.WindowDropdownSupportDonate);
                ImGui.EndDisabled();
            }
            ImGui.EndChild();
        }

        /// <summary>
        ///     Draw the logs dropdown.
        /// </summary>
        private void _drawLogs()
        {
            ImGui.BeginChild("Logs");
            ImGui.TextDisabled("Logs");
            ImGui.Separator();

            // length
            if (PluginService.EventLogManager.EventLog.Count == 0)
                ImGui.TextWrapped("No logs to display, check back later.");
            else
            {
                try
                {
                    ImGui.BeginTable("LogsTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Hideable);
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn("Time");
                    ImGui.TableSetupColumn("Type");
                    ImGui.TableSetupColumn("Message");
                    ImGui.TableHeadersRow();

                    foreach (var log in PluginService.EventLogManager.EventLog.OrderByDescending(x => x.timestamp))
                    {
                        if (log.type == Managers.EventLogManager.EventLogType.Warning)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                        }
                        else if (log.type == Managers.EventLogManager.EventLogType.Error)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                        }

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(log.timestamp.ToString("HH:mm:ss"));
                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text(log.type.ToString());
                        ImGui.TableSetColumnIndex(2);
                        ImGui.Text(log.message);
                        ImGui.PopStyleColor();
                    }

                    ImGui.EndTable();
                    ImGui.EndChild();
                }
                catch { /* Prevent ImGui errors when the window is smaller than the table */ }
            }
        }
    }
}