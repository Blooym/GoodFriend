using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoodFriend.Client.Http;
using GoodFriend.Client.Responses;
using GoodFriend.Client.Responses.Enums;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class AnnouncementModule : ModuleBase
    {
        /// <inheritdoc />
        public override string Name => "Announcements";

        /// <inheritdoc />
        public override string? Description => "Announcements allow for instance administrators to communicate important information to players in real-time. You can control what kind of announcements you receive in the settings or send your own announcements if you are an instance administrator.";

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Notifications;

        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private AnnouncementModuleConfig Config { get; set; } = ModuleConfigBase.Load<AnnouncementModuleConfig>();

        /// <summary>
        ///     Whether or not the client is allowed to send announcements.
        /// </summary>
        private bool CanSendAnnouncements { get; set; }

        /// <summary>
        ///     Whether or not a validation request is currently in progress.
        /// </summary>
        private bool IsValidationInProgress { get; set; }

        /// <summary>
        ///     The text to send as an announcement.
        /// </summary>
        private string sendAnnouncementText = string.Empty;

        /// <summary>
        ///     The kind of announcement to send.
        /// </summary>
        private AnnouncementKind sendAnnouncementKind = AnnouncementKind.Informational;

        /// <inheritdoc />
        protected override void EnableAction()
        {
            if (DalamudInjections.ClientState.IsLoggedIn && IsAnnouncementStreamDisconnected())
            {
                ApiClient.AnnouncementStream.Connect();
            }

            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;

            ApiClient.AnnouncementStream.OnStreamMessage += this.OnAnnouncementStreamMessage;
            ApiClient.AnnouncementStream.OnStreamException += this.OnAnnouncementStreamException;

            this.ValidateAuthToken();
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            if (IsAnnouncementStreamConnected())
            {
                ApiClient.AnnouncementStream.Disconnect();
            }

            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.ClientState.Logout -= this.OnLogout;

            ApiClient.AnnouncementStream.OnStreamMessage -= this.OnAnnouncementStreamMessage;
            ApiClient.AnnouncementStream.OnStreamException -= this.OnAnnouncementStreamException;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            // Stream status
            SiGui.Heading("Announcement Stream");
            SiGui.Text("Connection State:");
            ImGui.SameLine();
            switch (ApiClient.AnnouncementStream.ConnectionState)
            {
                case SSEConnectionState.Connected:
                    SiGui.TextColoured(Colours.Success, "Connected");
                    break;
                case SSEConnectionState.Connecting:
                    SiGui.TextColoured(Colours.Warning, "Connecting");
                    break;
                case SSEConnectionState.Disconnected:
                    SiGui.TextColoured(Colours.Error, "Disconnected");
                    break;
                case SSEConnectionState.Disconnecting:
                    SiGui.TextColoured(Colours.Warning, "Disconnecting");
                    break;
                case SSEConnectionState.Exception:
                    SiGui.TextColoured(Colours.Error, "Error");
                    break;
            }
            ImGui.Dummy(Spacing.SectionSpacing);

            // Options
            SiGui.Heading("Announcement Receive Options");
            foreach (var kind in Enum.GetValues<AnnouncementKind>())
            {
                var isEnabled = this.Config.EnabledAnnouncements.ContainsKey(kind) && this.Config.EnabledAnnouncements[kind];
                if (SiGui.Checkbox($"Show '{kind}' announcements", ref isEnabled))
                {
                    if (!this.Config.EnabledAnnouncements.ContainsKey(kind))
                    {
                        this.Config.EnabledAnnouncements.Add(kind, isEnabled);
                    }
                    else
                    {
                        this.Config.EnabledAnnouncements[kind] = isEnabled;
                    }
                }
            }

            // Send announcement
            ImGui.Dummy(Spacing.SectionSpacing);
            SiGui.Heading("Send Announcement");
            if (!this.CanSendAnnouncements)
            {
                var tokenSet = !string.IsNullOrWhiteSpace(Services.PluginConfiguration.ApiConfig.ApiKey);
                SiGui.TextWrapped("You must be an instance administrator with a valid authentication token set in the plugin settings to send announcements. If you are just using this plugin regularly, you can ignore this section.");
                ImGui.BeginDisabled(!tokenSet);
                if (ImGui.Button(tokenSet ? "Re-attempt authentication" : "No authentication token set"))
                {
                    this.ValidateAuthToken();
                }
                ImGui.EndDisabled();
            }
            else
            {
                SiGui.TextWrapped("Please note that all connected clients will receive your announcement as soon as you send it, be responsible.");
                if (ImGui.BeginCombo("Kind", this.sendAnnouncementKind.ToString()))
                {
                    foreach (var kind in Enum.GetValues<AnnouncementKind>())
                    {
                        if (ImGui.Selectable(kind.ToString(), kind == this.sendAnnouncementKind))
                        {
                            this.sendAnnouncementKind = kind;
                        }
                    }
                    ImGui.EndCombo();
                }
                SiGui.InputTextMultiline("Message", ref this.sendAnnouncementText, 430, new(0, 100));
                ImGui.BeginDisabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift) || string.IsNullOrWhiteSpace(this.sendAnnouncementText));
                if (ImGui.Button("Send"))
                {
                    this.SendAnnouncementBackground();
                }
                ImGui.EndDisabled();
                SiGui.TextDisabledWrapped("Hold shift to send announcement.");
            }
        }

        /// <summary>
        ///     Validates the authentication token with the API to ensure it is valid.
        /// </summary>
        private void ValidateAuthToken()
        {
            if (this.IsValidationInProgress)
            {
                return;
            }
            this.IsValidationInProgress = true;

            try
            {
                if (string.IsNullOrWhiteSpace(Services.PluginConfiguration.ApiConfig.ApiKey))
                {
                    this.CanSendAnnouncements = false;
                    Logger.Information("Authentication token is not set so cannot validate.");
                    return;
                }

                var validateRequest = ApiClient.ValidateAuthToken(Services.PluginConfiguration.ApiConfig.ApiKey);
                this.CanSendAnnouncements = validateRequest.Item1;
                Logger.Information($"Authentication token validation check returned: {validateRequest.Item2.StatusCode} - {validateRequest.Item2.ReasonPhrase}");
            }
            catch (Exception e)
            {
                Logger.Warning($"Failed to validate authentication token: {e}");
                this.CanSendAnnouncements = false;
            }
            finally
            {
                this.IsValidationInProgress = false;
            }
        }

        /// <summary>
        ///     Sends an announcement on another thread.
        /// </summary>
        private void SendAnnouncementBackground()
            => Task.Run(() =>
                {
                    var outcome = ApiClient.SendAnnouncement(new()
                    {
                        AuthenticationToken = Services.PluginConfiguration.ApiConfig.ApiKey,
                        Kind = this.sendAnnouncementKind,
                        Message = this.sendAnnouncementText,
                        Cause = AnnouncementCause.Manual,
                    });
                    if (outcome.IsSuccessStatusCode)
                    {
                        Logger.Information($"Announcement sent - Content: {this.sendAnnouncementText} - Kind: {this.sendAnnouncementKind} - Response: {outcome.ReasonPhrase}");
                        ChatHelper.Print("Announcement sucessfully sent.");
                    }
                    else
                    {
                        Logger.Warning($"Failed to send announcement: {outcome.ReasonPhrase}");
                        ChatHelper.PrintError($"Failed to send announcement: {outcome.ReasonPhrase}");
                    }
                    this.sendAnnouncementText = string.Empty;
                });

        /// <summary>
        ///     Called when an announcement is received.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rawEvent"></param>
        private void OnAnnouncementStreamMessage(object? _, AnnouncementStreamUpdate announcement)
        {
            if (string.IsNullOrWhiteSpace(announcement.Message))
            {
                return;
            }

            var shouldShow = this.Config.EnabledAnnouncements[announcement.Kind];
            if (!shouldShow)
            {
                return;
            }

            switch (announcement.Kind)
            {
                case AnnouncementKind.Critical:
                    ChatHelper.PrintImportant($"Announcement: {announcement.Message}");
                    break;
                default:
                    ChatHelper.Print($"Announcement: {announcement.Message}");
                    break;
            }
        }

        /// <summary>
        ///     Attempts to connect to relevant event streams when the player logs in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogin(object? sender, EventArgs e)
        {
            if (IsAnnouncementStreamDisconnected())
            {
                ApiClient.AnnouncementStream.Connect();
            }
        }

        /// <summary>
        ///     Attempts to disconnect from relevant event streams when the player logs out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (IsAnnouncementStreamConnected())
            {
                ApiClient.AnnouncementStream.Disconnect();
            }
        }

        /// <summary>
        ///     Handles an exception from the announcement stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnnouncementStreamException(object? sender, Exception e) => Logger.Error($"Exception received from announcement stream: {e}");

        /// <summary>
        ///     If the announcement stream is connected or connecting.
        /// </summary>
        /// <returns></returns>
        private static bool IsAnnouncementStreamConnected() => ApiClient.AnnouncementStream.ConnectionState is SSEConnectionState.Connected or SSEConnectionState.Connecting;

        /// <summary>
        ///     If the announcement stream is disconnected or disconnecting.
        /// </summary>
        /// <returns></returns>
        private static bool IsAnnouncementStreamDisconnected() => ApiClient.AnnouncementStream.ConnectionState is SSEConnectionState.Disconnected or SSEConnectionState.Disconnecting;

        /// <summary>
        ///     Configuration for the world change module.
        /// </summary>
        internal sealed class AnnouncementModuleConfig : ModuleConfigBase
        {
            /// <inheritdoc />
            public override uint Version { get; protected set; }

            /// <inheritdoc />
            protected override string Identifier { get; set; } = "AnnouncementModule";

            /// <summary>
            ///    The announcements that should be shown.
            /// </summary>
            public Dictionary<AnnouncementKind, bool> EnabledAnnouncements { get; set; } = new()
            {
                { AnnouncementKind.Critical, true },
                { AnnouncementKind.Informational, true },
                { AnnouncementKind.Maintenance, true },
            };
        }
    }
}
