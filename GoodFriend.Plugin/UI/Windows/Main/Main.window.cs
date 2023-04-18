using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using GoodFriend.Plugin.Common;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.UI.ImGuiBasicComponents;
using GoodFriend.Plugin.UI.ImGuiFullComponents.ConnectionStatusComponent;
using GoodFriend.Plugin.UI.ImGuiFullComponents.TextInput;
using GoodFriend.Plugin.Utils;
using ImGuiNET;
using DalamudNotifications = Dalamud.Interface.Internal.Notifications;

namespace GoodFriend.Plugin.UI.Windows.Main
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
        public MainWindow() : base(Constants.PluginName)
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
            var statusPageUrl = MainPresenter.Metadata?.StatusUrl;
            ConnectionStatusComponent.Draw("ConnectionPageStatusComponent", new Vector2(0, 70 * ImGui.GetIO().FontGlobalScale));

            // Status button
            ImGui.BeginDisabled(statusPageUrl == null);
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsStatus, PrimaryWindow.DropdownOptionsStatusTooltip(statusPageUrl ?? "NULL"), new Vector2((ImGui.GetWindowWidth() / 3) - 10, 0)))
            {
                if (statusPageUrl != null)
                {
                    Util.OpenLink(statusPageUrl);
                }
            }
            ImGui.EndDisabled();
            ImGui.SameLine();

            // Donate button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSupport, PrimaryWindow.DropdownOptionsSupportTooltip, new Vector2((ImGui.GetWindowWidth() / 3) - 10, 0)))
            {
                this.presenter.ToggleVisibleDropdown(MainPresenter.VisibleDropdown.Donate);
            }
            ImGui.SameLine();

            // Settings button
            if (Tooltips.TooltipButton(PrimaryWindow.DropdownOptionsSettings, PrimaryWindow.DropdownOptionsSettingsTooltip, new Vector2((ImGui.GetWindowWidth() / 3) - 10, 0)))
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
                case MainPresenter.VisibleDropdown.None:
                    ImGui.SetWindowSize(new Vector2(450 * ImGui.GetIO().FontGlobalScale, 175 * ImGui.GetIO().FontGlobalScale));
                    ImGuiHelpers.CenteredText(Constants.Build.Version.ToString());
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
                    var notificationType = Services.Configuration.EventStreamUpdateMessagesConfig.NotificationType;
                    var loginMessage = Services.Configuration.EventStreamUpdateMessagesConfig.FriendLoggedInMessage;
                    var logoutMessage = Services.Configuration.EventStreamUpdateMessagesConfig.FriendLoggedOutMessage;
                    var hideSameFC = Services.Configuration.EventStreamUpdateFilterConfig.HideSameFC;
                    var hideDifferentHomeworld = Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentHomeworld;
                    var hideDifferentWorld = Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentWorld;
                    var hideDifferentTerritory = Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentTerritory;
                    var hideDifferentDatacenter = Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentDatacenter;

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
                                Services.Configuration.EventStreamUpdateFilterConfig.HideSameFC = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideSameFC))
                            {
                                Services.Configuration.EventStreamUpdateFilterConfig.HideSameFC = false;
                            }

                            Services.Configuration.Save();
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
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentWorld = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentWorld))
                            {
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentWorld = false;
                            }

                            Services.Configuration.Save();
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
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentHomeworld = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentHomeworld))
                            {
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentHomeworld = false;
                            }

                            Services.Configuration.Save();
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
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentDatacenter = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentDatacenter))
                            {
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentDatacenter = false;
                            }

                            Services.Configuration.Save();
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
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentTerritory = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !hideDifferentTerritory))
                            {
                                Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentTerritory = false;
                            }

                            Services.Configuration.Save();
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
                                    Services.Configuration.EventStreamUpdateMessagesConfig.NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), type);
                                    Services.Configuration.Save();
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
                                Services.Configuration.EventStreamUpdateMessagesConfig.FriendLoggedInMessage = loginMessage.Trim();
                                Services.Configuration.Save();
                            }
                        }, 255);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsLoginMessageTooltip);
                        ImGui.TableNextRow();

                        // Logout message
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsLogoutMessage);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
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
                                Services.Configuration.EventStreamUpdateMessagesConfig.FriendLoggedOutMessage = logoutMessage.Trim();
                                Services.Configuration.Save();
                            }
                        }, 255);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsLogoutMessageTooltip);

                        ImGui.EndTable();
                    }

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                // "Advanced" Settings
                if (ImGui.BeginTabItem(PrimaryWindow.SettingsTabAdvanced))
                {
                    var showAPIEvents = Services.Configuration.ApiConfig.ShowAPIEvents;
                    var apiUrl = Services.Configuration.ApiConfig.APIUrl.ToString();

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
                                Services.Configuration.ApiConfig.ShowAPIEvents = true;
                            }

                            if (ImGui.Selectable(PrimaryWindow.DropdownSettingsDisabled, !showAPIEvents))
                            {
                                Services.Configuration.ApiConfig.ShowAPIEvents = false;
                            }

                            Services.Configuration.Save();
                            ImGui.EndCombo();
                        }
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPINotificationsTooltip);
                        ImGui.TableNextRow();

                        // API URL
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(PrimaryWindow.DropdownSettingsAPIUrl);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.SetNextItemWidth(-1);
                        TextInput.Draw("##APIUrl", ref apiUrl, () =>
                        {
                            if (apiUrl == Services.Configuration.ApiConfig.APIUrl.ToString())
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
                                Services.Configuration.ApiConfig.APIUrl = new Uri(apiUrl);
                                Services.Configuration.Save();
                                this.presenter.RestartToApply = true;
                            }
                            else
                            {
                                Notifications.Show(PrimaryWindow.DropdownSettingsAPIUrlInvalid, NotificationType.Toast, DalamudNotifications.NotificationType.Error);

                                Services.Configuration.ApiConfig.APIUrl = Constants.Links.DefaultAPIUrl;
                                Services.Configuration.Save();
                            }
                        }, 500);
                        Tooltips.AddTooltipHover(PrimaryWindow.DropdownSettingsAPIUrlTooltip);
                        ImGui.TableNextRow();
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
            if (Tooltips.TooltipButton($"{PrimaryWindow.DropdownSupportDonate}##devSupport", Constants.Links.KoFi.ToString()))
            {
                Util.OpenLink(Constants.Links.KoFi.ToString());
            }
            ImGui.Dummy(new Vector2(0, 5));

            // Get the donation page from the API, if it doesnt exist then just disable the button.
            var donationPageUrl = MainPresenter.Metadata?.DonateUrl;
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
    }
}
