using System;
using GoodFriend.Client.Http;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class AnnouncementStreamConnectionModule : ModuleBase
    {
        /// <summary>
        ///     The last time a heartbeat was received from the announcement stream.
        /// </summary>
        private DateTime LastHeartbeatTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The last time an event was received from the announcement stream.
        /// </summary>
        private DateTime LasEventTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The number of events received from the announcement stream.
        /// </summary>
        private uint EventsReceived { get; set; }

        /// <summary>
        ///     The number of heartbeats received from the announcement stream.
        /// </summary>
        private uint HeartbeatsReceived { get; set; }

        /// <inheritdoc />
        public override string Name => "Announcement Stream Connection";

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Connectivity;

        /// <inheritdoc />
        public override uint LoadPriority { get; } = 1;

        /// <inheritdoc />
        protected override void EnableAction()
        {
            if (DalamudInjections.ClientState.IsLoggedIn && IsAnnouncementStreamDisconnected())
            {
                ApiClient.AnnouncementStream.Connect();
            }

            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;
            ApiClient.AnnouncementStream.OnStreamHeartbeat += this.OnAnnouncementStreamHeartbeat;
            ApiClient.AnnouncementStream.OnStreamMessage += this.OnAnnouncementStreamEvent;
            ApiClient.AnnouncementStream.OnStreamException += this.OnAnnouncementStreamException;
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
            ApiClient.AnnouncementStream.OnStreamHeartbeat -= this.OnAnnouncementStreamHeartbeat;
            ApiClient.AnnouncementStream.OnStreamMessage -= this.OnAnnouncementStreamEvent;
            ApiClient.AnnouncementStream.OnStreamException -= this.OnAnnouncementStreamException;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            SiGui.TextWrapped(Strings.Modules_PlayerStreamConnectionModule_ConnectionStatus);
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
            SiGui.Heading("Connection Statistics");
            SiGui.TextWrapped($"Events received: {this.EventsReceived}");
            SiGui.TextWrapped($"Last event: {this.LasEventTime:HH:mm:ss}");
            SiGui.TextWrapped($"Heartbeats received: {this.HeartbeatsReceived}");
            SiGui.TextWrapped($"Last heartbeat: {this.LastHeartbeatTime:HH:mm:ss}");
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
        ///     Handles a heartbeat from the announcement stream.
        /// </summary>
        /// <param name="sender"></param>
        private void OnAnnouncementStreamHeartbeat(object? sender)
        {
            Logger.Verbose("Heartbeat received from announcement stream.");
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
            Logger.Verbose($"Event received from announcement stream.");
            this.LasEventTime = DateTime.Now;
            this.EventsReceived++;
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
    }
}
