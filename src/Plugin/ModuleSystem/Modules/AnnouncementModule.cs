using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using GoodFriend.Client.Http;
using GoodFriend.Client.Http.Requests;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Client.Http.Responses.Enums;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class AnnouncementModule : ModuleBase
    {
        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private AnnouncementModuleConfig Config { get; set; } = ModuleConfigBase.Load<AnnouncementModuleConfig>();

        /// <summary>
        ///     The SSE stream for announcements.
        /// </summary>
        private SSEClient<AnnouncementStreamUpdate> AnnouncementSSEStream { get; set; } = null!;

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
        ///     The channel to send the announcement to.
        /// </summary>
        private string sendAnnouncementChannel = CustomAnnouncementChannels.GoodFriendDev;

        /// <summary>
        ///     The kind of announcement to send.
        /// </summary>
        private AnnouncementKind sendAnnouncementKind = AnnouncementKind.Informational;

        /// <inheritdoc />
        public override string Name => "Announcements";

        /// <inheritdoc />
        public override string? Description => "Announcements allow for instance administrators to communicate important information to players in real-time. You can control what kind of announcements you receive in the settings or send your own announcements if you are an instance administrator.";

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Notifications;

        /// <inheritdoc />
        protected override void EnableAction()
        {
            this.AnnouncementSSEStream = GetAnnouncementStreamRequest.CreateSSEClient(HttpClient, new Timer(60000));
            if (DalamudInjections.ClientState.IsLoggedIn && this.IsAnnouncementStreamDisconnected())
            {
                this.AnnouncementSSEStream.Connect();
            }

            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;

            this.AnnouncementSSEStream.OnStreamMessage += this.OnAnnouncementStreamMessage;
            this.AnnouncementSSEStream.OnStreamException += this.OnAnnouncementStreamException;

            this.ValidateAuthToken();
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            if (this.IsAnnouncementStreamConnected())
            {
                this.AnnouncementSSEStream.Disconnect();
            }
            this.AnnouncementSSEStream.Dispose();

            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.ClientState.Logout -= this.OnLogout;

            this.AnnouncementSSEStream.OnStreamMessage -= this.OnAnnouncementStreamMessage;
            this.AnnouncementSSEStream.OnStreamException -= this.OnAnnouncementStreamException;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            this.DrawAnnouncementStreamState();
            ImGui.Dummy(Spacing.SectionSpacing);
            this.DrawReceiveOptions();
            ImGui.Dummy(Spacing.SectionSpacing);
            this.DrawSendAnnouncement();
        }

        private void DrawAnnouncementStreamState()
        {
            SiGui.Heading("Announcement Stream");
            SiGui.Text("Connection State:");
            ImGui.SameLine();
            switch (this.AnnouncementSSEStream.ConnectionState)
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
        }

        private void DrawReceiveOptions()
        {
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
        }

        private void DrawSendAnnouncement()
        {
            SiGui.Heading("Send Announcement");
            if (!this.CanSendAnnouncements)
            {
                var tokenSet = !string.IsNullOrWhiteSpace(Services.PluginConfiguration.ApiConfig.AuthKey);
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

                if (ImGui.BeginCombo("Channel", this.sendAnnouncementChannel == string.Empty ? "Everyone" : this.sendAnnouncementChannel))
                {
                    if (ImGui.Selectable("Everyone", this.sendAnnouncementChannel == string.Empty))
                    {
                        this.sendAnnouncementChannel = string.Empty;
                    }
                    foreach (var channel in CustomAnnouncementChannels.All)
                    {
                        if (ImGui.Selectable(channel, channel == this.sendAnnouncementChannel))
                        {
                            this.sendAnnouncementChannel = channel;
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
                if (string.IsNullOrWhiteSpace(Services.PluginConfiguration.ApiConfig.AuthKey))
                {
                    this.CanSendAnnouncements = false;
                    Logger.Information("Authentication token is not set so cannot validate.");
                    return;
                }
                var validateRequest = new PostValidateAuthRequest().Send(HttpClient, new()
                {
                    AuthenticationToken = Services.PluginConfiguration.ApiConfig.AuthKey
                });
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
                    var outcome = new PostAnnouncementRequest().Send(HttpClient, new()
                    {
                        AuthenticationToken = Services.PluginConfiguration.ApiConfig.AuthKey,
                        Kind = this.sendAnnouncementKind,
                        Message = this.sendAnnouncementText,
                        Cause = AnnouncementCause.Manual,
                        Channel = string.IsNullOrWhiteSpace(this.sendAnnouncementChannel) ? null : this.sendAnnouncementChannel,
                    });
                    if (outcome.IsSuccessStatusCode)
                    {
                        Logger.Information($"Announcement sent\nChannel:${this.sendAnnouncementChannel}\nMessage: {this.sendAnnouncementText}\nKind: {this.sendAnnouncementKind}\nResult: {outcome.ReasonPhrase}");
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
            Logger.Information($"Received announcement: {announcement}");
            if (string.IsNullOrWhiteSpace(announcement.Message))
            {
                Logger.Information("Received announcement with no message, ignoring.");
                return;
            }

            // Check that the channel is valid for the current mode
            var isTesting = DalamudInjections.PluginInterface.IsTesting;
            var isDev = DalamudInjections.PluginInterface.IsDev;
            switch (announcement.Channel)
            {
                case CustomAnnouncementChannels.GoodFriendTesting when !isTesting && !isDev:
                    Logger.Information($"Ignoring announcement due to channel '{announcement.Channel}' when not in testing/dev mode");
                    return;
                case CustomAnnouncementChannels.GoodFriendRelease when isTesting || isDev:
                    Logger.Information($"Ignoring announcement due to channel '{announcement.Channel}' when in testing/dev mode");
                    return;
                case CustomAnnouncementChannels.GoodFriendDev when !isDev:
                    Logger.Information($"Ignoring announcement due to channel '{announcement.Channel}' when not in dev mode");
                    return;
            }

            var shouldShow = this.Config.EnabledAnnouncements[announcement.Kind];
            if (!shouldShow)
            {
                Logger.Information($"Received announcement of kind '{announcement.Kind}' but it is disabled, ignoring.");
                return;
            }

            // Show the message
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
            if (this.IsAnnouncementStreamDisconnected())
            {
                this.AnnouncementSSEStream.Connect();
            }
        }

        /// <summary>
        ///     Attempts to disconnect from relevant event streams when the player logs out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (this.IsAnnouncementStreamConnected())
            {
                this.AnnouncementSSEStream.Disconnect();
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
        private bool IsAnnouncementStreamConnected() => this.AnnouncementSSEStream.ConnectionState is SSEConnectionState.Connected or SSEConnectionState.Connecting;

        /// <summary>
        ///     If the announcement stream is disconnected or disconnecting.
        /// </summary>
        /// <returns></returns>
        private bool IsAnnouncementStreamDisconnected() => this.AnnouncementSSEStream.ConnectionState is SSEConnectionState.Disconnected or SSEConnectionState.Disconnecting;

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

        internal static class CustomAnnouncementChannels
        {
            /// <summary>
            ///     Testing users only.
            /// </summary>
            public const string GoodFriendTesting = "GOODFRIEND_TESTING";

            /// <summary>
            ///     Stable release users only.
            /// </summary>
            public const string GoodFriendRelease = "GOODFRIEND_RELEASE";

            /// <summary>
            ///     Dev plugin users only.
            /// </summary>
            public const string GoodFriendDev = "GOODFRIEND_DEV";

            /// <summary>
            ///     All custom announcement channels.
            /// </summary>
            public static string[] All { get; } = typeof(CustomAnnouncementChannels).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue()!)
                .ToArray();
        }
    }
}
