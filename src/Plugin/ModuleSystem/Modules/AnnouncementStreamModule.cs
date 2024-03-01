using System;
using GoodFriend.Client.Http;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class AnnouncementStreamModule : ModuleBase
    {
        /// <summary>
        ///     The last time a heartbeat was received from the player event stream.
        /// </summary>
        private DateTime LastHeartbeatTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The last time an event was received from the player event stream.
        /// </summary>
        private DateTime LastEventTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The number of events received from the player event stream.
        /// </summary>
        private uint EventsReceived { get; set; }

        /// <summary>
        ///     The number of heartbeats received from the player event stream.
        /// </summary>
        private uint HeartbeatsReceived { get; set; }

        /// <inheritdoc />
        public override string Name => "Announcements Connection";

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Connectivity;

        /// <inheritdoc />
        public override uint LoadPriority { get; } = 1;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            if (DalamudInjections.ClientState.IsLoggedIn && IsAnnouncementStreamDisconnected())
            {
                AnnouncementSseStream.Connect();
            }

            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;
            AnnouncementSseStream.OnStreamHeartbeat += this.OnAnnouncementStreamHeartbea2t;
            AnnouncementSseStream.OnStreamMessage += this.OnAnnouncementStreamEvent;
            AnnouncementSseStream.OnStreamException += this.OnAnnouncementStreamException;
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            if (IsAnnouncementStreamConnected())
            {
                AnnouncementSseStream.Connect();
            }

            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.ClientState.Logout -= this.OnLogout;
            AnnouncementSseStream.OnStreamHeartbeat -= this.OnAnnouncementStreamHeartbea2t;
            AnnouncementSseStream.OnStreamMessage -= this.OnAnnouncementStreamEvent;
            AnnouncementSseStream.OnStreamException -= this.OnAnnouncementStreamException;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            SiGui.TextWrapped(Strings.Modules_AnnouncementModule_ConnectionStatus);
            ImGui.SameLine();
            switch (AnnouncementSseStream.ConnectionState)
            {
                case SseConnectionState.Connected:
                    SiGui.TextColoured(Colours.Success, Strings.Modules_AnnouncementModule_ConnectionState_Connected);
                    break;
                case SseConnectionState.Connecting:
                    SiGui.TextColoured(Colours.Warning, Strings.Modules_AnnouncementModule_ConnectionState_Connecting);
                    break;
                case SseConnectionState.Disconnected:
                    SiGui.TextColoured(Colours.Error, Strings.Modules_AnnouncementModule_ConnectionState_Disconnected);
                    break;
                case SseConnectionState.Disconnecting:
                    SiGui.TextColoured(Colours.Warning, Strings.Modules_AnnouncementModule_ConnectionState_Disconnecting);
                    break;
                case SseConnectionState.Exception:
                    SiGui.TextColoured(Colours.Error, Strings.Modules_AnnouncementModule_ConnectionState_Exception);
                    break;
            }
            ImGui.Dummy(Spacing.SectionSpacing);
            SiGui.Heading(Strings.Modules_PlayerStreamConnectionModule_ConnectionStatistics);
            SiGui.TextWrapped(string.Format(Strings.Modules_PlayerStreamConnectionModule_ConnectionStatistics_EventsTotal, this.EventsReceived));
            SiGui.TextWrapped(string.Format(Strings.Modules_PlayerStreamConnectionModule_ConnectionStatistics_LastEvent, $"{this.LastEventTime:HH:mm:ss}"));
            SiGui.TextWrapped(string.Format(Strings.Modules_PlayerStreamConnectionModule_ConnectionStatistics_HeartbeatsTotal, this.HeartbeatsReceived));
            SiGui.TextWrapped(string.Format(Strings.Modules_PlayerStreamConnectionModule_ConnectionStatistics_LastHeartbeat, $"{this.LastHeartbeatTime:HH:mm:ss}"));
        }

        /// <summary>
        ///     Attempts to connect to relevant event streams when the player logs in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogin()
        {
            if (IsAnnouncementStreamDisconnected())
            {
                AnnouncementSseStream.Connect();
            }
        }

        /// <summary>
        ///     Attempts to disconnect from relevant event streams when the player logs out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogout()
        {
            if (IsAnnouncementStreamConnected())
            {
                AnnouncementSseStream.Disconnect();
            }
        }

        /// <summary>
        ///     Handles a heartbeat from the player event stream.
        /// </summary>
        /// <param name="sender"></param>
        private void OnAnnouncementStreamHeartbea2t(object? sender)
        {
            Logger.Verbose("Heartbeat received from event stream.");
            this.LastHeartbeatTime = DateTime.Now;
            this.HeartbeatsReceived++;
        }

        /// <summary>
        ///     Handles an event from the announcement stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnnouncementStreamEvent(object? sender, AnnouncementStreamUpdate e)
        {
            Logger.Verbose($"Event received from player event stream.");
            this.LastEventTime = DateTime.Now;
            this.EventsReceived++;
        }

        /// <summary>
        ///     Handles an exception from the player event stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnnouncementStreamException(object? sender, Exception e) => Logger.Error($"Exception received from event stream: {e}");

        /// <summary>
        ///     If the player event stream is connected or connecting.
        /// </summary>
        /// <returns></returns>
        private static bool IsAnnouncementStreamConnected() => AnnouncementSseStream.ConnectionState is SseConnectionState.Connected or SseConnectionState.Connecting;

        /// <summary>
        ///     If the player event stream is disconnected or disconnecting.
        /// </summary>
        /// <returns></returns>
        private static bool IsAnnouncementStreamDisconnected() => AnnouncementSseStream.ConnectionState is SseConnectionState.Disconnected or SseConnectionState.Disconnecting;
    }
}
