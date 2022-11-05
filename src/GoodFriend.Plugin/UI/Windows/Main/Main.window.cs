using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using GoodFriend.Base;
using GoodFriend.Enums;
using GoodFriend.Localization;
using GoodFriend.UI.ImGuiComponents;
using GoodFriend.UI.ImGuiFullComponents.ConnectionStatusComponent;
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
        public MainWindow() : base(PluginConstants.pluginName)
        {
            Flags |= ImGuiWindowFlags.NoScrollbar;
            Flags |= ImGuiWindowFlags.NoResize;
            Flags |= ImGuiWindowFlags.NoDocking;
            presenter = new MainPresenter();
        }

        /// <summary>
        ///     Dispose of the window and all associated resources.
        /// </summary>
        public void Dispose()
        {
            presenter.Dispose();
        }

        /// <summary>
        ///     Draws all elements and components for the settings window, including sub-components.
        /// </summary>
#pragma warning disable CS1717
        public override void Draw()
        {
            string? statusPageUrl = MainPresenter.Metadata?.StatusPageUrl;
            ConnectionStatusComponent.Draw("ConnectionPageStatusComponent", new Vector2(0, 70 * ImGui.GetIO().FontGlobalScale));

            // Status button
            ImGui.BeginDisabled(statusPageUrl == null);
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsStatus, PrimaryWindow.DropdownOptionsStatusTooltip(statusPageUrl ?? "NULL"), new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
#pragma warning disable CS8604
                Util.OpenLink(statusPageUrl);
#pragma warning restore CS8604
            }
            ImGui.EndDisabled();
            ImGui.SameLine();

            // Donate button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSupport, PrimaryWindow.DropdownOptionsSupportTooltip, new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Donate);
            }

            ImGui.SameLine();

            // Event log button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsEventLog, PrimaryWindow.DropdownOptionsEventLogTooltip, new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Logs);
            }

            ImGui.SameLine();

            // Settings button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSettings, PrimaryWindow.DropdownOptionsSettingsTooltip, new Vector2((ImGui.GetWindowWidth() / 4) - 10, 0)))
            {
                presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Settings);
            }

            ImGui.Dummy(new Vector2(0, 10));

            // Handle the option dropdowns - size is handled outside of the sub-components for clarity.
            switch (presenter.CurrentVisibleDropdown)
            {
                case MainPresenter.VisibleDropdown.Settings:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 420 * ImGui.GetIO().FontGlobalScale));
                    DrawSettings();
                    break;
                case MainPresenter.VisibleDropdown.Donate:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 400 * ImGui.GetIO().FontGlobalScale));
                    DrawSupport();
                    break;
                case MainPresenter.VisibleDropdown.Logs:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 400 * ImGui.GetIO().FontGlobalScale));
                    DrawLogs();
                    break;
                case MainPresenter.VisibleDropdown.None:
                    ImGui.SetWindowSize(new Vector2(410 * ImGui.GetIO().FontGlobalScale, 160 * ImGui.GetIO().FontGlobalScale));
                    ImGui.SetCursorPosX(((ImGui.GetWindowWidth() - ImGui.CalcTextSize($"Plugin: {PluginConstants.version} · APIClient {PluginService.APIClientManager.Version}").X) / 2) - ImGui.GetStyle().WindowPadding.X);
                    ImGui.TextDisabled($"Plugin: {PluginConstants.version} · APIClient: {PluginService.APIClientManager.Version}");
                    break;
                default:
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

            if (presenter.RestartToApply)
            {
                Colours.TextWrappedColoured(Colours.Warning, PrimaryWindow.DropdownSettingsRestartRequired);
            }

            if (ImGui.BeginTabBar("SettingsDropdownTabBar"))
            {
                // "Basic" Settings
                if (ImGui.BeginTabItem("General"))
                {
                    string? notificationType = Enum.GetName(typeof(NotificationType), PluginService.Configuration.NotificationType);
                    string loginMessage = PluginService.Configuration.FriendLoggedInMessage;
                    string logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
                    bool hideSameFC = PluginService.Configuration.HideSameFC;
                    bool hideDifferentHomeworld = PluginService.Configuration.HideDifferentHomeworld;
                    bool hideDifferentWorld = PluginService.Configuration.HideDifferentWorld;
                    bool hideDifferentTerritory = PluginService.Configuration.HideDifferentTerritory;
                    bool hideDifferentDatacenter = PluginService.Configuration.HideDifferentDatacenter;


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
                        if (ImGui.BeginCombo("##NotificationType", notificationType))
                        {
                            foreach (string type in Enum.GetNames(typeof(NotificationType)))
                            {
                                if (ImGui.Selectable(LoCExtensions.GetLocalizedName((NotificationType)Enum.Parse(typeof(NotificationType), type)), notificationType == type))
                                {
                                    PluginService.Configuration.NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), type);
                                    PluginService.Configuration.Save();
                                }
                                Tooltips.AddTooltipHover(LoCExtensions.GetLocalizedDescription((NotificationType)Enum.Parse(typeof(NotificationType), type)));
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
#pragma warning disable CA1806 // Do not ignore method results
                            try { string.Format(loginMessage, "test"); }
#pragma warning restore CA1806 // Do not ignore method results
                            catch { error = true; }

                            if (error || !loginMessage.Contains("{0}"))
                            {
                                if (ImGui.IsItemDeactivatedAfterEdit())
                                {
                                    Notifications.Show(PrimaryWindow.DropdownInvalidLoginMessage, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                                }
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
#pragma warning disable CA1806 // Do not ignore method results
                            try { string.Format(logoutMessage, "test"); }
#pragma warning restore CA1806 // Do not ignore method results
                            catch { error = true; }

                            if (error || !logoutMessage.Contains("{0}"))
                            {
                                if (ImGui.IsItemDeactivatedAfterEdit())
                                {
                                    Notifications.Show(PrimaryWindow.DropdownInvalidLogoutMessage, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                                }
                            }
                            else
                            {
                                PluginService.Configuration.FriendLoggedOutMessage = logoutMessage.Trim();
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
                    bool showAPIEvents = PluginService.Configuration.ShowAPIEvents;
                    string APIUrl = PluginService.Configuration.APIUrl.ToString();
                    string friendshipCode = PluginService.Configuration.FriendshipCode;
                    string apiAuth = PluginService.Configuration.APIAuthentication;
                    SaltMethods saltMethod = PluginService.Configuration.SaltMethod;

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
                            if (APIUrl == PluginService.Configuration.APIUrl.ToString())
                            {
                                return;
                            }

                            bool error = false;
#pragma warning disable CA1806 // Do not ignore method results
                            try { new Uri(APIUrl); }
#pragma warning restore CA1806 // Do not ignore method results
                            catch { error = true; }

                            if (!error)
                            {
                                PluginService.Configuration.APIUrl = new Uri(APIUrl);
                                PluginService.Configuration.Save();
                                presenter.RestartToApply = true;
                            }
                            else
                            {
                                if (ImGui.IsItemDeactivatedAfterEdit())
                                {
                                    Notifications.Show(PrimaryWindow.DropdownSettingsAPIUrlInvalid, NotificationType.Toast, DalamudNotifications.NotificationType.Error);
                                }

                                PluginService.Configuration.APIUrl = PluginConstants.defaultAPIUrl;
                                PluginService.Configuration.Save();
                            }
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
                            presenter.RestartToApply = true;
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPITokenTooltip);
                        ImGui.TableNextRow();


                        // Salt Method
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsSaltMethod);
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginCombo("##SaltMethod", LoCExtensions.GetLocalizedName(saltMethod)))
                        {
                            foreach (string type in Enum.GetNames(typeof(SaltMethods)))
                            {
                                if (ImGui.Selectable(LoCExtensions.GetLocalizedName(Enum.Parse<SaltMethods>(type)), saltMethod == Enum.Parse<SaltMethods>(type)))
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

                    presenter.dialogManager.Draw();
                    if (ImGui.Button("Export Localizable", new Vector2(ImGui.GetWindowWidth() - 20, 0)))
                    {
                        presenter.dialogManager.OpenFolderDialog("Export LOC", MainPresenter.OnDirectoryPicked);
                    }

                    ImGui.TextDisabled("Detected Friends - Click to copy ContentID hash");
                    ImGui.BeginChild("##Friends");
                    foreach (Managers.FriendList.FriendListEntry friend in presenter.GetFriendList())
                    {
                        if (ImGui.Selectable(friend.Name.ToString()))
                        {
                            ImGui.SetClipboardText(Hashing.HashSHA512(friend.ContentId.ToString()));
                            Notifications.Show($"Copied {friend.Name}'s ContentID hash to clipboard", NotificationType.Toast);
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
            _ = ImGui.BeginChild("SupportDropdown");
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportTitle);
            ImGui.Separator();
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportFlavourText);
            ImGui.Dummy(new Vector2(0, 10));


            // Support the developer button
            ImGui.TextDisabled(PrimaryWindow.DropdownSupportDeveloper);
            ImGui.TextWrapped(PrimaryWindow.DropdownSupportDeveloperDescription);
            if (Tooltips.TooltipButton($"{PrimaryWindow.DropdownSupportDonate}##devSupport", PluginConstants.pluginDevSupportUrl.ToString()))
            {
                Util.OpenLink(PluginConstants.pluginDevSupportUrl.ToString());
            }
            ImGui.Dummy(new Vector2(0, 5));


            string? donationPageUrl = MainPresenter.Metadata?.DonationPageUrl;
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
                _ = ImGui.Button(PrimaryWindow.DropdownSupportDonate);
                ImGui.EndDisabled();
            }
            ImGui.EndChild();
        }

        /// <summary>
        ///     Draw the logs dropdown.
        /// </summary>
        private static void DrawLogs()
        {
            _ = ImGui.BeginChild("Logs");
            ImGui.TextDisabled("Logs");
            ImGui.Separator();

            if (MainPresenter.EventLog.EventLog.Count <= 0)
            {
                ImGui.TextWrapped("No logs to display, check back later.");
            }
            else
            {
                if (ImGui.BeginTable("LogsTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.BordersInner | ImGuiTableFlags.Hideable))
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableTime);
                    ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableType);
                    ImGui.TableSetupColumn(PrimaryWindow.DropdownLogsTableMessage);
                    ImGui.TableHeadersRow();

                    foreach (Managers.EventLogManager.EventLogEntry? log in MainPresenter.EventLog.EventLog.OrderByDescending(x => x.Timestamp))
                    {
                        if (log.Type == Managers.EventLogManager.EventLogType.Warning)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                        }
                        else if (log.Type == Managers.EventLogManager.EventLogType.Error)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                        }

                        ImGui.TableNextRow();
                        _ = ImGui.TableSetColumnIndex(0);
                        ImGui.Text(log.Timestamp.ToString("HH:mm:ss"));
                        _ = ImGui.TableSetColumnIndex(1);
                        ImGui.Text(log.Type.ToString());
                        _ = ImGui.TableSetColumnIndex(2);
                        ImGui.Text(log.Message);
                        ImGui.PopStyleColor();
                    }
                    ImGui.EndTable();
                }
            }
            ImGui.EndChild();
        }
    }
}