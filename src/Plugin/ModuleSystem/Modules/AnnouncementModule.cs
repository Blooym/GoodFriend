using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GoodFriend.Client.Http.Requests;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Client.Http.Responses.Enums;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
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
        public override string Name => Strings.Modules_AnnouncementModule_Name;

        /// <inheritdoc />
        public override string? Description => Strings.Modules_AnnouncementModule_Description;

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Notifications;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            AnnouncementSseStream.OnStreamMessage += this.OnAnnouncementStreamMessage;
            AnnouncementSseStream.OnStreamException += this.OnAnnouncementStreamException;
            this.ValidateAuthToken();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            AnnouncementSseStream.OnStreamMessage -= this.OnAnnouncementStreamMessage;
            AnnouncementSseStream.OnStreamException -= this.OnAnnouncementStreamException;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            this.DrawReceiveOptions();
            ImGui.Dummy(Spacing.SectionSpacing);
            this.DrawSendAnnouncement();
        }
        private void DrawReceiveOptions()
        {
            SiGui.Heading(Strings.Modules_AnnouncementModule_ReceiveOptions);
            foreach (var kind in Enum.GetValues<AnnouncementKind>())
            {
                var isEnabled = this.Config.EnabledAnnouncements.ContainsKey(kind) && this.Config.EnabledAnnouncements[kind];
                if (SiGui.Checkbox(string.Format(Strings.Modules_AnnouncementModule_ReceiveOptions_ShowKind, kind), ref isEnabled))
                {
                    if (!this.Config.EnabledAnnouncements.ContainsKey(kind))
                    {
                        this.Config.EnabledAnnouncements.Add(kind, isEnabled);
                    }
                    else
                    {
                        this.Config.EnabledAnnouncements[kind] = isEnabled;
                    }
                    this.Config.Save();
                }
            }
        }

        private void DrawSendAnnouncement()
        {
            SiGui.Heading(Strings.Modules_AnnouncementModule_SendAnnouncement_Header);
            if (!this.CanSendAnnouncements)
            {
                var tokenSet = !string.IsNullOrWhiteSpace(Services.PluginConfiguration.ApiConfig.AuthKey);
                SiGui.TextWrapped(Strings.Modules_AnnouncementModule_SendAnnouncement_NoAuthNotAdmin);
                ImGui.BeginDisabled(!tokenSet);
                if (ImGui.Button(tokenSet ? Strings.Modules_AnnouncementModule_SendAnnouncement_Reauth : Strings.Modules_AnnouncementModule_SendAnnouncement_NoAuth))
                {
                    this.ValidateAuthToken();
                }
                ImGui.EndDisabled();
            }
            else
            {
                SiGui.TextWrapped(Strings.Modules_AnnouncementModule_SendAnnouncement_Notice);
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
                if (ImGui.Button(Strings.Modules_AnnouncementModule_SendAnnouncement_Send))
                {
                    this.SendAnnouncementBackground();
                }
                ImGui.EndDisabled();
                SiGui.TextDisabledWrapped(Strings.Modules_AnnouncementModule_SendAnnouncement_HoldShift);
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
                        ChatHelper.Print(Strings.Modules_AnnouncementModule_SendAnnouncement_SendSuccess);
                    }
                    else
                    {
                        Logger.Warning($"Failed to send announcement: {outcome.ReasonPhrase}");
                        ChatHelper.PrintError(string.Format(Strings.Modules_AnnouncementModule_SendAnnouncement_SendError, outcome.ReasonPhrase));
                    }
                    this.sendAnnouncementText = string.Empty;
                });

        /// <summary>
        ///     Called when an announcement is received.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="announcement"></param>
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
                    ChatHelper.PrintImportant(string.Format(Strings.Modules_AnnouncementModule_AnnouncementText, announcement.Message));
                    break;
                default:
                    ChatHelper.Print(string.Format(Strings.Modules_AnnouncementModule_AnnouncementText, announcement.Message));
                    break;
            }
        }

        /// <summary>
        ///     Handles an exception from the announcement stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnnouncementStreamException(object? sender, Exception e) => Logger.Error($"Exception received from announcement stream: {e}");

        /// <summary>
        ///     Configuration for the world change module.
        /// </summary>
        private sealed class AnnouncementModuleConfig : ModuleConfigBase
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

        private static class CustomAnnouncementChannels
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
