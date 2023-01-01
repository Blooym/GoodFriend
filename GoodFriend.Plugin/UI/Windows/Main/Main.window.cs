using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using GoodFriend.Base;
using GoodFriend.Localization;
using GoodFriend.Managers;
using GoodFriend.UI.ImGuiBasicComponents;
using GoodFriend.UI.ImGuiFullComponents.ConnectionStatusComponent;
using GoodFriend.UI.ImGuiFullComponents.TextInput;
using GoodFriend.Utils;
using ImGuiNET;
using DalamudNotifications = Dalamud.Interface.Internal.Notifications;

namespace GoodFriend.UI.Windows.Main
{
    /// <summary>
    ///     The settings window for the plugin.
    /// </summary>
    public sealed class MainWindow : Window, IDisposable
    {
        /// <summary>
        ///     The presenter associated with the window, handles all logic and data.
        /// </summary>
        private readonly MainPresenter presenter;

        /// <summary>
        ///     Instantiate a new settings window.
        /// </summary>
        public MainWindow() : base(PluginConstants.PluginName)
        {
            this.Flags |= ImGuiWindowFlags.NoScrollbar;
            this.Flags |= ImGuiWindowFlags.NoResize;
            this.Flags |= ImGuiWindowFlags.NoDocking;
            this.presenter = new MainPresenter();
        }

        /// <summary>
        ///     Dispose of the window and all associated resources.
        /// </summary>
        public void Dispose() => this.presenter.Dispose();

        /// <summary>
        ///     Draws all elements and components for the settings window, including sub-components.
        /// </summary>
        public override void Draw()
        {
            var statusPageUrl = MainPresenter.Metadata?.StatusPageUrl;
            ConnectionStatusComponent.Draw("ConnectionPageStatusComponent", new Vector2(0, 70 * ImGui.GetIO().FontGlobalScale));

            // Status button
            ImGui.BeginDisabled(statusPageUrl == null);
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsStatus, PrimaryWindow.DropdownOptionsStatusTooltip(statusPageUrl ?? "NULL"), new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                if (statusPageUrl != null)
                {
                    Util.OpenLink(statusPageUrl);
                }
            }
            ImGui.EndDisabled();
            ImGui.SameLine();

            // Donate button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSupport, PrimaryWindow.DropdownOptionsSupportTooltip, new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Donate);
            }
            ImGui.SameLine();

            // Event log button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsEventLog, PrimaryWindow.DropdownOptionsEventLogTooltip, new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Logs);
            }

            ImGui.SameLine();

            // Settings button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSettings, PrimaryWindow.DropdownOptionsSettingsTooltip, new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Settings);
            }

            ImGui.Dummy(new Vector2(0, 10));

            // Handle the option dropdowns - size is handled outside of the sub-components for clarity.
            switch (this.presenter.CurrentVisibleDropdown)
            {
                case MainPresenter.VisibleDropdown.Settings:
                    ImGui.SetWindowSize(new Vector2(450 * ImGui.GetIO().FontGlobalScale, 420 * ImGui.GetIO().FontGlobalScale));
                    this.DrawSettings();
                    break;
                case MainPresenter.VisibleDropdown.Donate:
                    ImGui.SetWindowSize(new Vector2(450 * ImGui.GetIO().FontGlobalScale, 420 * ImGui.GetIO().FontGlobalScale));
                    DrawSupport();
                    break;
                case MainPresenter.VisibleDropdown.Logs:
                    ImGui.SetWindowSize(new Vector2(450 * ImGui.GetIO().FontGlobalScale, 420 * ImGui.GetIO().FontGlobalScale));
                    this.DrawEventLog();
                    break;
                case MainPresenter.VisibleDropdown.None:
                    ImGui.SetWindowSize(new Vector2(450 * ImGui.GetIO().FontGlobalScale, 175 * ImGui.GetIO().FontGlobalScale));
                    var message = $"Plugin: {PluginConstants.Version} · APIClient: {APIClientManager.Version} · {(Common.IsOfficialSource ? PrimaryWindow.SettingsOfficialBuild : PrimaryWindow.SettingsUnofficialBuild)}";
                    ImGui.SetCursorPosX(((ImGui.GetWindowWidth() - ImGui.CalcTextSize(message).X) / 2) - ImGui.GetStyle().WindowPadding.X);
                    ImGui.TextDisabled(message);
                    break;
            }
        }

        /// <summary>
        ///     Draw the settings dropdown.
        /// </summary>
        private void DrawSettings()
        {
            ImGui.TextDisabled("Settings");
            ImGui.BeginChild("SettingsDropdown");

            if (this.presenter.RestartToApply)
            {
                Colours.TextWrappedColoured(Colours.Warning, PrimaryWindow.DropdownSettingsRestartRequired);
            }

            if (ImGui.BeginTabBar("SettingsDropdownTabBar"))
            {
                // "Basic" Settings
                if (ImGui.BeginTabItem(PrimaryWindow.SettingsTabGeneral))
                {
                    var notificationType = PluginService.Configuration.NotificationType;
                    var loginMessage = PluginService.Configuration.FriendLoggedInMessage;
                    var logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
                    var hideSameFC = PluginService.Configuration.HideSameFC;
                    var hideDifferentHomeworld = PluginService.Configuration.HideDifferentHomeworld;
                    var hideDifferentWorld = PluginService.Configuration.HideDifferentWorld;
                    var hideDifferentTerritory = PluginService.Configuration.HideDifferentTerritory;
                    var hideDifferentDatacenter = PluginService.Configuration.HideDifferentDatacenter;

                    ImGui.BeginChild("GeneralSettings");
                    if (ImGui.BeginTable("###GeneralSettingsTable", 2))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableNextRow();

                        // Ignore FC Members
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreFC);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##IgnoreFCMembers", hideSameFC ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideSameFC))
                            {
                                PluginService.Configuration.HideSameFC = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideSameFC))
                            {
                                PluginService.Configuration.HideSameFC = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreFCTooltip);
                        ImGui.TableNextRow();

                        // Ignore different worlds.
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreDiffWorlds);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##IgnoreDifferentWorlds", hideDifferentWorld ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideDifferentWorld))
                            {
                                PluginService.Configuration.HideDifferentWorld = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentWorld))
                            {
                                PluginService.Configuration.HideDifferentWorld = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreDiffWorldsTooltip);
                        ImGui.TableNextRow();

                        // Ignore different homeworlds.
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreDiffHomeworlds);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##IgnoreDifferentHomeworlds", hideDifferentHomeworld ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideDifferentHomeworld))
                            {
                                PluginService.Configuration.HideDifferentHomeworld = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentHomeworld))
                            {
                                PluginService.Configuration.HideDifferentHomeworld = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreDiffHomeworldsTooltip);
                        ImGui.TableNextRow();

                        // Ignore different datacenters.
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreDiffDatacenters);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##IgnoreDifferentDatacenters", hideDifferentDatacenter ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideDifferentDatacenter))
                            {
                                PluginService.Configuration.HideDifferentDatacenter = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentDatacenter))
                            {
                                PluginService.Configuration.HideDifferentDatacenter = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreDiffDatacentersTooltip);
                        ImGui.TableNextRow();

                        // Ignore different territories.
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsIgnoreDiffTerritories);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##IgnoreDifferentTerritory", hideDifferentTerritory ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, hideDifferentTerritory))
                            {
                                PluginService.Configuration.HideDifferentTerritory = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentTerritory))
                            {
                                PluginService.Configuration.HideDifferentTerritory = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsIgnoreDiffTerritoriesTooltip);
                        ImGui.TableNextRow();

                        // Notification Type
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsNotificationType);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##NotificationType", notificationType.ToString()))
                        {
                            foreach (var type in Enum.GetNames(typeof(NotificationType)))
                            {
                                if (ImGui.Selectable(type, notificationType == Enum.Parse<NotificationType>(type)))
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
                        ImGui.SetNextItemWidth(-1);
                        TextInput.Draw("##LoginMessage", ref loginMessage, () =>
                        {
                            var error = false;
#pragma warning disable CA1806 // Do not ignore method results
                            try
                            { string.Format(loginMessage, "Test"); }
#pragma warning restore CA1806 // Do not ignore method results
                            catch { error = true; }

                            if (error || !loginMessage.Contains("{0}"))
                            {
                                Notifications.Show(PrimaryWindow.DropdownInvalidLoginMessage, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                            }
                            else
                            {
                                PluginService.Configuration.FriendLoggedInMessage = loginMessage.Trim();
                                PluginService.Configuration.Save();
                            }
                        }, 255);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsLoginMessageTooltip);
                        ImGui.TableNextRow();
                        TextInput.Draw("##LogoutMessage", ref logoutMessage, () =>
                        {
                            var error = false;
#pragma warning disable CA1806 // Do not ignore method results
                            try
                            { string.Format(logoutMessage, "Test"); }
#pragma warning restore CA1806 // Do not ignore method results
                            catch { error = true; }

                            if (error || !logoutMessage.Contains("{0}"))
                            {
                                Notifications.Show(PrimaryWindow.DropdownInvalidLogoutMessage, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                            }
                            else
                            {
                                PluginService.Configuration.FriendLoggedOutMessage = logoutMessage.Trim();
                                PluginService.Configuration.Save();
                            }
                        }, 255);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsLogoutMessageTooltip);

                        ImGui.TableNextRow();
                        ImGui.EndTable();
                    }
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                // "Advanced" Settings
                if (ImGui.BeginTabItem(PrimaryWindow.SettingsTabAdvanced))
                {
                    var showAPIEvents = PluginService.Configuration.ShowAPIEvents;
                    var apiUrl = PluginService.Configuration.APIUrl.ToString();
                    var friendshipCode = PluginService.Configuration.FriendshipCode;
                    var apiAuth = PluginService.Configuration.APIAuthentication;
                    var saltMethod = PluginService.Configuration.SaltMethod;

                    ImGui.BeginChild("##AdvancedSettings");
                    if (ImGui.BeginTable("SettingsTable", 2))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableNextRow();

                        // API Connection Notifications
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPINotifications);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##APIConnectionNotifications", showAPIEvents ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, showAPIEvents))
                            {
                                PluginService.Configuration.ShowAPIEvents = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !showAPIEvents))
                            {
                                PluginService.Configuration.ShowAPIEvents = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPINotificationsTooltip);
                        ImGui.TableNextRow();

                        // Friendship code
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsFriendshipCode);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        TextInput.Draw("##FriendshipCode", ref friendshipCode, () =>
                        {
                            PluginService.Configuration.FriendshipCode = friendshipCode.Trim();
                            PluginService.Configuration.Save();
                        }, 255);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsFriendshipCodeTooltip);
                        ImGui.TableNextRow();

                        // API URL
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPIUrl);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        TextInput.Draw("##APIUrl", ref apiUrl, () =>
                        {
                            if (apiUrl == PluginService.Configuration.APIUrl.ToString())
                            {
                                return;
                            }

                            var error = false;
#pragma warning disable CA1806 // Do not ignore method results
                            try
                            { new Uri(apiUrl); }
#pragma warning restore CA1806 // Do not ignore method results
                            catch { error = true; }

                            if (!error)
                            {
                                PluginService.Configuration.APIUrl = new Uri(apiUrl);
                                PluginService.Configuration.Save();
                                this.presenter.RestartToApply = true;
                            }
                            else
                            {
                                Notifications.Show(PrimaryWindow.DropdownSettingsAPIUrlInvalid, NotificationType.Toast, DalamudNotifications.NotificationType.Error);

                                PluginService.Configuration.APIUrl = PluginConstants.DefaultAPIUrl;
                                PluginService.Configuration.Save();
                            }
                        }, 500);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPIUrlTooltip);
                        ImGui.TableNextRow();

                        // API Token
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPIToken);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        TextInput.Draw("##APIAuth", ref apiAuth, () =>
                        {
                            PluginService.Configuration.APIAuthentication = apiAuth.Trim();
                            PluginService.Configuration.Save();
                            this.presenter.RestartToApply = true;
                        }, 500);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPITokenTooltip);
                        ImGui.TableNextRow();

                        // Salt Method
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsSaltMethod);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##SaltMethod", saltMethod.ToString()))
                        {
                            foreach (var type in Enum.GetNames(typeof(SaltMethods)))
                            {
                                if (ImGui.Selectable(type, saltMethod == Enum.Parse<SaltMethods>(type)))
                                {
                                    PluginService.Configuration.SaltMethod = (SaltMethods)Enum.Parse(typeof(SaltMethods), type);
                                    PluginService.Configuration.Save();
                                }
                            }
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsSaltMethodTooltip);
                        ImGui.TableNextRow();

                        // Don't cache friends list (dropdown "on" or "off")
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsFriendslistCaching);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.BeginCombo("##DontCacheFriends", PluginService.Configuration.FriendslistCaching ? PrimaryWindow.DropdownSettingsEnabled : PrimaryWindow.DropdownSettingsDisabled))
                        {
                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsEnabled, PluginService.Configuration.FriendslistCaching))
                            {
                                PluginService.Configuration.FriendslistCaching = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !PluginService.Configuration.FriendslistCaching))
                            {
                                PluginService.Configuration.FriendslistCaching = false;
                            }

                            PluginService.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsFriendslistCachingTooltip);


                        ImGui.EndTable();
                    }

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

#if DEBUG
                // "Debug" Settings
                if (ImGui.BeginTabItem("Developer"))
                {
                    ImGui.BeginChild("##DeveloperSettings");

                    this.presenter.DialogManager.Draw();
                    if (ImGui.Button("Export Localizable", new Vector2(ImGui.GetWindowWidth(), 0)))
                    {
                        this.presenter.DialogManager.OpenFolderDialog("Export LOC", MainPresenter.OnDirectoryPicked);
                    }

                    ImGui.TextDisabled("Detected Friends - Click to copy ContentID hash & salt");
                    ImGui.BeginChild("##Friends");

                    var friendList = this.presenter.GetFriendList();
                    if (friendList.Count == 0)
                    {
                        ImGui.TextWrapped("Unable to pull friends list data, the cache might be unavailable or disabled.");
                    }
                    else
                    {
                        foreach (var friend in friendList)
                        {
                            if (ImGui.Selectable(friend.Name.ToString()))
                            {
                                var rand = CryptoUtil.GenerateRandom(32);
                                ImGui.SetClipboardText($"ContentID: {CryptoUtil.HashSHA512(friend.ContentId.ToString(), rand)} Salt: {rand}");
                                Notifications.Show($"Copied {friend.Name}'s ContentID hash & salt to clipboard", NotificationType.Toast, DalamudNotifications.NotificationType.Success);
                            }
                        }
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
        private static void DrawSupport()
        {
            // Top caption and flavour text.
            ImGui.BeginChild("##SupportDropdown");
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportTitle);
            ImGui.Separator();
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportFlavourText);
            ImGui.Dummy(new Vector2(0, 10));

            // Support the plugin developer button.
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportDeveloper);
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportDeveloperDescription);
            if (Tooltips.TooltipButton($"{PrimaryWindow.DropdownSupportDonate}##devSupport", PluginConstants.PluginDevSupportUrl.ToString()))
            {
                Util.OpenLink(PluginConstants.PluginDevSupportUrl.ToString());
            }
            ImGui.Dummy(new Vector2(0, 5));

            // Get the donation page from the API, if it doesnt exist then just disable the button.
            var donationPageUrl = MainPresenter.Metadata?.DonationPageUrl;
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportAPIHost);
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportAPIHostDescription);
            if (donationPageUrl != null)
            {
                try
                {
                    Uri uri = new(donationPageUrl);
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
        ///     Draw the event log dropdown.
        /// </summary>
        private void DrawEventLog()
        {
            ImGui.BeginChild("##EventlogDropdown");
            ImGui.TextDisabled(PrimaryWindow.DropdownLogsTitle);
            ImGui.Separator();

            // Clear logs button.
            if (ImGui.Button(PrimaryWindow.DropdownLogsClearAll))
            {
                MainPresenter.EventLog.ClearAll();
            }
            ImGui.SameLine();

            // Dropdown to change the log level to display.
            if (ImGui.BeginCombo("##LogFilter", this.presenter.EventLogFilter.ToString()))
            {
                foreach (var type in Enum.GetNames(typeof(EventLogManager.EventLogType)))
                {
                    if (ImGui.Selectable(type, this.presenter.EventLogFilter == Enum.Parse<EventLogManager.EventLogType>(type)))
                    {
                        this.presenter.EventLogFilter = (EventLogManager.EventLogType)Enum.Parse(typeof(EventLogManager.EventLogType), type);
                    }
                }
                ImGui.EndCombo();
            }
            Tooltips.AddTooltipHover(PrimaryWindow.DropdownLogsFilterTooltip);
            ImGui.Dummy(new Vector2(0, 5));

            // Filter by the log level, if none exist at the level or higher then display a message.
            var eventsAtLevel = MainPresenter.EventLog.GetEntries(this.presenter.EventLogFilter, true);
            if (!eventsAtLevel.Any())
            {
                ImGui.TextWrapped(PrimaryWindow.DropdownLogsNoLogs);
            }

            // Otherwise, display a table of the logs at that level or higher.
            else if (ImGui.BeginTable("LogsTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Hideable))
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableTime);
                ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableType);
                ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableMessage, ImGuiTableColumnFlags.NoHide);
                ImGui.TableHeadersRow();

                // only show the filtered log or anything above it (by enum value)
                foreach (var log in eventsAtLevel.OrderByDescending(x => x.Timestamp))
                {
                    // Push a colour depending on the type of log it is.
                    if (log.Type == EventLogManager.EventLogType.Warning)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Colours.Warning);
                    }
                    else if (log.Type == EventLogManager.EventLogType.Error)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Colours.Error);
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(log.Timestamp.ToString("HH:mm:ss"));
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(log.Type.ToString());
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text(log.Message);
                    ImGui.PopStyleColor();

                    // Context menu to copy or delete the log.
                    if (ImGui.BeginPopupContextItem($"##LogContextMenu{log.ID}"))
                    {
                        if (ImGui.Selectable(PrimaryWindow.DropdownLogsCopy))
                        {
                            ImGui.SetClipboardText(log.Message);
                            Notifications.Show(PrimaryWindow.DropdownLogsCopySuccess, NotificationType.Toast, toastType: DalamudNotifications.NotificationType.Success);
                        }
                        ImGui.Separator();
                        if (ImGui.Selectable(PrimaryWindow.DropdownLogsDelete))
                        {
                            MainPresenter.EventLog.RemoveEntry(log.ID);
                        }
                        ImGui.EndPopup();
                    }
                }
                ImGui.EndTable();
            }
            ImGui.EndChild();
        }
    }
}
