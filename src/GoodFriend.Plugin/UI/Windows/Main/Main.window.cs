namespace GoodFriend.UI.Windows.Main
{
    using System;
    using System.Linq;
    using System.Numerics;
    using GoodFriend.Base;
    using GoodFriend.Enums;
    using GoodFriend.Utils;
    using GoodFriend.UI.Components;
    using Dalamud.Utility;
    using Dalamud.Interface.Windowing;
    using DalamudNotifications = Dalamud.Interface.Internal.Notifications;
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
        ///     Dispose of the window and all associated resources.
        /// </summary>
        public void Dispose() => this.presenter.Dispose();

        /// <summary>
        ///     Draws all elements and components for the settings window, including sub-components.
        /// </summary>
#pragma warning disable CS1717
        public override void Draw()
        {
            var statusPageUrl = this.presenter.metadata?.statusPageUrl;
            var newApiUrl = this.presenter.metadata?.newApiUrl;
            Components.ConnectionStatusComponent.Draw("ConnectionPageStatusComponent", new Vector2(0, 70 * ImGui.GetIO().FontGlobalScale));

            // Status button
            ImGui.BeginDisabled(statusPageUrl == null);
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsStatus, PrimaryWindow.DropdownOptionsStatusTooltip(statusPageUrl ?? "NULL"), new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
            {
#pragma warning disable CS8604
                Util.OpenLink(statusPageUrl);
#pragma warning restore CS8604
            }
            ImGui.EndDisabled();
            ImGui.SameLine();

            // Donate button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSupport, PrimaryWindow.DropdownOptionsSupportTooltip, new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Donate);
            ImGui.SameLine();

            // Event log button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsEventLog, PrimaryWindow.DropdownOptionsEventLogTooltip, new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Logs);
            ImGui.SameLine();

            // Settings button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSettings, PrimaryWindow.DropdownOptionsSettingsTooltip, new Vector2(ImGui.GetWindowWidth() / 4 - 10, 0)))
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Settings);
            ImGui.Dummy(new Vector2(0, 10));

            // Handle the option dropdowns - size is handled outside of the sub-components for clarity.
            switch (this.presenter.visibleDropdown)
            {
                case MainPresenter.VisibleDropdown.Settings:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 380 * ImGui.GetIO().FontGlobalScale));
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
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 160 * ImGui.GetIO().FontGlobalScale));
                    ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize($"Plugin: {PStrings.version} · APIClient {PluginService.APIClientManager.version}").X) / 2 - ImGui.GetStyle().WindowPadding.X);
                    ImGui.TextDisabled($"Plugin: {PStrings.version} · APIClient: {PluginService.APIClientManager.version}");
                    break;
            }
        }

        /// <summary>
        ///     Draw the settings dropdown.
        /// </summary>
        private void _drawSettings()
        {
            ImGui.TextDisabled("Settings");
            ImGui.BeginChild("SettingsDropdown");

            if (this.presenter.restartToApply == true)
                Colours.TextWrappedColoured(Colours.Warning, PrimaryWindow.DropdownSettingsRestartRequired);



            if (ImGui.BeginTabBar("SettingsDropdownTabBar"))
            {
                // "Basic" Settings
                if (ImGui.BeginTabItem("General"))
                {
                    var notificationType = Enum.GetName(typeof(NotificationType), PluginService.Configuration.NotificationType);
                    var loginMessage = PluginService.Configuration.FriendLoggedInMessage;
                    var logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
                    var hideSameFC = PluginService.Configuration.HideSameFC;
                    var hideDifferentHomeworld = PluginService.Configuration.HideDifferentHomeworld;
                    var hideDifferentTerritory = PluginService.Configuration.HideDifferentTerritory;

                    ImGui.BeginChild("GeneralSettings");
                    if (ImGui.BeginTable("GeneralSettingsTable", 2))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableNextRow();

                        // Ignore FC Members
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreFC);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##IgnoreFCMembers", hideSameFC ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideSameFC))
                                PluginService.Configuration.HideSameFC = true;
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideSameFC))
                                PluginService.Configuration.HideSameFC = false;
                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreFCTooltip);
                        ImGui.TableNextRow();

                        // Ignore different homeworlds.
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreDiffHomeworlds);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##IgnoreDifferentHomeworlds", hideDifferentHomeworld ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideDifferentHomeworld))
                                PluginService.Configuration.HideDifferentHomeworld = true;
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentHomeworld))
                                PluginService.Configuration.HideDifferentHomeworld = false;
                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreDiffHomeworldsTooltip);
                        ImGui.TableNextRow();

                        // Ignore different territories.
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreDiffTerritories);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##IgnoreDifferentTerritory", hideDifferentTerritory ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideDifferentTerritory))
                                PluginService.Configuration.HideDifferentTerritory = true;
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentTerritory))
                                PluginService.Configuration.HideDifferentTerritory = false;
                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreDiffTerritoriesTooltip);
                        ImGui.TableNextRow();

                        // Notification Type
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsNotificationType);
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
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsNotificationTypeTooltip);
                        ImGui.TableNextRow();


                        // Login message
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsLoginMessage);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##LoginMessage", ref loginMessage, 255))
                        {
                            bool error = false;
                            try { string.Format(loginMessage, "test"); }
                            catch { error = true; }

                            if (error || !loginMessage.Contains("{0}"))
                            {
                                if (ImGui.IsItemDeactivatedAfterEdit()) Notifications.Show(PrimaryWindow.DropdownInvalidLoginMessage, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                            }
                            else
                            {
                                PluginService.Configuration.FriendLoggedInMessage = loginMessage.Trim();
                                PluginService.Configuration.Save();
                            }
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsLoginMessageTooltip);
                        ImGui.TableNextRow();

                        // Logout message
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsLogoutMessage);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##LogoutMessage", ref logoutMessage, 255))
                        {
                            bool error = false;
                            try { string.Format(logoutMessage, "test"); }
                            catch { error = true; }

                            if (error || !logoutMessage.Contains("{0}"))
                            {
                                if (ImGui.IsItemDeactivatedAfterEdit()) Notifications.Show(PrimaryWindow.DropdownInvalidLogoutMessage, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                            }
                            else
                            {
                                PluginService.Configuration.FriendLoggedInMessage = logoutMessage.Trim();
                                PluginService.Configuration.Save();
                            }
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsLogoutMessageTooltip);

                        ImGui.TableNextRow();
                        ImGui.EndTable();
                    }
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                // "Advanced" Settings
                if (ImGui.BeginTabItem("Advanced"))
                {
                    var showAPIEvents = PluginService.Configuration.ShowAPIEvents;
                    var APIUrl = PluginService.Configuration.APIUrl.ToString();
                    var friendshipCode = PluginService.Configuration.FriendshipCode;
                    var apiAuth = PluginService.Configuration.APIAuthentication;
                    var saltMethod = PluginService.Configuration.SaltMethod;

                    ImGui.BeginChild("AdvancedSettings");
                    if (ImGui.BeginTable("SettingsTable", 2))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableNextRow();

                        // API Connection Notifications
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPINotifications);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##APIConnectionNotifications", showAPIEvents ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, showAPIEvents))
                                PluginService.Configuration.ShowAPIEvents = true;
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !showAPIEvents))
                                PluginService.Configuration.ShowAPIEvents = false;
                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPINotificationsTooltip);
                        ImGui.TableNextRow();


                        // Friendship code
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsFriendshipCode);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##FriendshipCode", ref friendshipCode, 255))
                        {
                            PluginService.Configuration.FriendshipCode = friendshipCode.Trim();
                            PluginService.Configuration.Save();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsFriendshipCodeTooltip);
                        ImGui.TableNextRow();


                        // API URL
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPIUrl);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##APIUrl", ref APIUrl, 255))
                        {
                            if (APIUrl == PluginService.Configuration.APIUrl.ToString()) return;

                            bool error = false;
                            try { new Uri(APIUrl); }
                            catch { error = true; }

                            if (!error) { PluginService.Configuration.APIUrl = new Uri(APIUrl); PluginService.Configuration.Save(); this.presenter.restartToApply = true; }
                            else { PluginService.Configuration.APIUrl = PStrings.defaultAPIUrl; PluginService.Configuration.Save(); }
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPIUrlTooltip);
                        ImGui.TableNextRow();


                        // API Token
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPIToken);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.InputText("##APIAuth", ref apiAuth, 255))
                        {
                            PluginService.Configuration.APIAuthentication = apiAuth.Trim();
                            PluginService.Configuration.Save();
                            this.presenter.restartToApply = true;
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPITokenTooltip);
                        ImGui.TableNextRow();


                        // Salt Method
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsSaltMethod);
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
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsSaltMethodTooltip);
                        ImGui.EndTable();
                    }
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
                        this.presenter.dialogManager.OpenFolderDialog("Export LOC", this.presenter.OnDirectoryPicked);

                    ImGui.TextDisabled("Detected Friends - Click to copy ContentID hash");
                    ImGui.BeginChild("##Friends");
                    foreach (var friend in this.presenter.GetFriendList())
                        if (ImGui.Selectable(friend.Name.ToString()))
                        {
                            ImGui.SetClipboardText(Hashing.HashSHA512(friend.ContentId.ToString()));
                            Notifications.Show($"Copied {friend.Name.ToString()}'s ContentID hash to clipboard", NotificationType.Toast);
                        }
                    ImGui.EndChild();

                    ImGui.EndChild();
                }
#endif
                ImGui.EndTabBar();
            }
            ImGui.EndChild();
        }

        /// <summary>
        ///     Draw the support dropdown.
        /// </summary>
        private void _drawSupport()
        {
            // Top caption and flavour text.
            ImGui.BeginChild("SupportDropdown");
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportTitle);
            ImGui.Separator();
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportFlavourText);
            ImGui.Dummy(new Vector2(0, 10));


            // Support the developer button
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportDeveloper);
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportDeveloperDescription);
            if (Tooltips.TooltipButton($"{PrimaryWindow.DropdownSupportDonate}##devSupport", PStrings.pluginDevSupportUrl.ToString()))
            {
                Util.OpenLink(PStrings.pluginDevSupportUrl.ToString());
            }
            ImGui.Dummy(new Vector2(0, 5));


            var donationPageUrl = this.presenter.metadata?.donationPageUrl;
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportAPIHost);
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportAPIHostDescription);
            if (donationPageUrl != null)
            {
                try
                {
                    var uri = new Uri(donationPageUrl);
                    if (Tooltips.TooltipButton($"{PrimaryWindow.DropdownSupportDonate}##hostSupport", donationPageUrl))
                    {
                        Util.OpenLink(donationPageUrl);
                    }
                }
                catch
                {
                    ImGui.TextWrapped(PrimaryWindow.DropdownSupportAPIHostInvalidUri);
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button(PrimaryWindow.DropdownSupportDonate);
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

            if (this.presenter.EventLog.EventLog.Count <= 0)
                ImGui.TextWrapped("No logs to display, check back later.");
            else
            {
                if (ImGui.BeginTable("LogsTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Hideable))
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableTime);
                    ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableType);
                    ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableMessage);
                    ImGui.TableHeadersRow();

                    foreach (var log in this.presenter.EventLog.EventLog.OrderByDescending(x => x.timestamp))
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
                }
            }
            ImGui.EndChild();
        }
    }
}