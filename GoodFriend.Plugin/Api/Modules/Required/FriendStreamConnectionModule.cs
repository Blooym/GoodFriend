using System;
using GoodFriend.Client;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.Api.Modules.Required
{
    internal sealed class FriendStreamConnectionModule : ApiRequiredModule
    {
        /// <summary>
        ///     The last time a heartbeat was recieved from the player event stream.
        /// </summary>
        private DateTime LastHeartbeatTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The last time an event was recieved from the player event stream.
        /// </summary>
        private DateTime LasEventTime { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The number of events recieved from the player event stream.
        /// </summary>
        private uint EventsRecieved { get; set; }

        /// <summary>
        ///     The number of heartbeats recieved from the player event stream.
        /// </summary>
        private uint HeartbeatsRecieved { get; set; }

        /// <inheritdoc />
        public override string Name => "Friend Stream Connection";

        /// <inheritdoc />
        public override ApiModuleTag Tag => ApiModuleTag.Information;

        /// <inheritdoc />
        public override uint LoadPriority { get; } = 1;

        /// <inheritdoc />
        protected override void EnableAction()
        {
            if (DalamudInjections.ClientState.IsLoggedIn && IsPlayerStreamDisconnected())
            {
                ApiClient.ConnectToPlayerEventStream();
            }

            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;
            ApiClient.OnPlayerStreamHeartbeat += this.OnPlayerStreamHeartbeat;
            ApiClient.OnPlayerStreamMessage += this.OnPlayerStreamEvent;
            ApiClient.OnPlayerStreamException += this.OnPlayerStreamException;
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            if (IsPlayerStreamConnected())
            {
                ApiClient.DisconnectFromPlayerEventStream();
            }

            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.ClientState.Logout -= this.OnLogout;
            ApiClient.OnPlayerStreamHeartbeat -= this.OnPlayerStreamHeartbeat;
            ApiClient.OnPlayerStreamMessage -= this.OnPlayerStreamEvent;
            ApiClient.OnPlayerStreamException -= this.OnPlayerStreamException;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            SiGui.TextWrapped("Connection Status:");
            ImGui.SameLine();
            switch (ApiClient.PlayerStreamConnectionState)
            {
                case EventStreamConnectionState.Connected:
                    SiGui.TextColoured(Colours.Success, "Connected");
                    SiGui.TextWrapped("You are currently recieving events from other friends and are fully connected to the player event stream.");
                    break;
                case EventStreamConnectionState.Connecting:
                    SiGui.TextColoured(Colours.Warning, "Connecting...");
                    SiGui.TextWrapped("Attempting to connect you to the player event stream. Please wait...");
                    break;
                case EventStreamConnectionState.Disconnected:
                    SiGui.TextColoured(Colours.Error, "Disconnected");
                    SiGui.TextWrapped("You are not currently recieving events from other friends and are not connected to the player event stream.");
                    break;
                case EventStreamConnectionState.Disconnecting:
                    SiGui.TextColoured(Colours.Warning, "Disconnecting...");
                    SiGui.TextWrapped("Attempting to disconnect you from the player event stream. Please wait...");
                    break;
                case EventStreamConnectionState.Exception:
                    SiGui.TextColoured(Colours.Error, "Error");
                    SiGui.TextWrapped("Something went wrong when attempting to connect to the player event stream! It may be due to a network issue or the API may be down, check /xllog for more information.");
                    break;
            }
            ImGui.Dummy(Spacing.SectionSpacing);
            SiGui.Heading("Stream Information");
            SiGui.TextWrapped($"Events Recieved: {this.EventsRecieved}");
            SiGui.TextWrapped($"Last Event: {this.LasEventTime:HH:mm:ss}");
            SiGui.TextWrapped($"Heartbeats Recieved: {this.HeartbeatsRecieved}");
            SiGui.TextWrapped($"Last Heartbeat: {this.LastHeartbeatTime:HH:mm:ss}");
        }

        /// <summary>
        ///     Attempts to connect to relevant event streams when the player logs in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogin(object? sender, EventArgs e)
        {
            if (IsPlayerStreamDisconnected())
            {
                ApiClient.ConnectToPlayerEventStream();
            }
        }

        /// <summary>
        ///     Attempts to disconnect from relevant event streams when the player logs out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (IsPlayerStreamConnected())
            {
                ApiClient.DisconnectFromPlayerEventStream();
            }
        }

        /// <summary>
        ///     Handles a heartbeat from the player event stream.
        /// </summary>  
        /// <param name="sender"></param>
        private void OnPlayerStreamHeartbeat(object? sender)
        {
            Logger.Verbose("Heartbeat recieved from event stream.");
            this.LastHeartbeatTime = DateTime.Now;
            this.HeartbeatsRecieved++;
        }

        /// <summary>
        ///     Handles an event from the player event stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerStreamEvent(object? sender, PlayerEventStreamUpdate e)
        {
            Logger.Verbose($"Event recieved from player event stream.");
            this.LasEventTime = DateTime.Now;
            this.EventsRecieved++;
        }

        /// <summary>
        ///     Handles an exception from the player event stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerStreamException(object? sender, Exception e) => Logger.Error($"Exception recieved from event stream: {e}");

        /// <summary>
        ///     If the player event stream is connected or connecting.
        /// </summary>
        /// <returns></returns>
        private static bool IsPlayerStreamConnected() => ApiClient.PlayerStreamConnectionState is EventStreamConnectionState.Connected or EventStreamConnectionState.Connecting;

        /// <summary>
        ///     If the player event stream is disconnected or disconnecting.
        /// </summary>
        /// <returns></returns>
        private static bool IsPlayerStreamDisconnected() => ApiClient.PlayerStreamConnectionState is EventStreamConnectionState.Disconnected or EventStreamConnectionState.Disconnecting;
    }
}
