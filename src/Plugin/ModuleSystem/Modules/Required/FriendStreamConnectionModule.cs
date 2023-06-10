using System;
using GoodFriend.Client;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules.Required
{
    internal sealed class FriendStreamConnectionModule : RequiredModuleBase
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
        public override string Name => Strings.Modules_FriendStreamConnectionModule_Name;

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Connectivity;

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
            SiGui.TextWrapped(Strings.Modules_FriendStreamConnectionModule_ConnectionStatus);
            ImGui.SameLine();
            switch (ApiClient.PlayerStreamConnectionState)
            {
                case EventStreamConnectionState.Connected:
                    SiGui.TextColoured(Colours.Success, Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Connected);
                    SiGui.TextWrapped(Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Connected_Description);
                    break;
                case EventStreamConnectionState.Connecting:
                    SiGui.TextColoured(Colours.Warning, Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Connecting);
                    SiGui.TextWrapped(Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Connecting_Description);
                    break;
                case EventStreamConnectionState.Disconnected:
                    SiGui.TextColoured(Colours.Error, Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Disconnected);
                    SiGui.TextWrapped(Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Disconnected_Description);
                    break;
                case EventStreamConnectionState.Disconnecting:
                    SiGui.TextColoured(Colours.Warning, Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Disconnecting);
                    SiGui.TextWrapped(Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Disconnecting_Description);
                    break;
                case EventStreamConnectionState.Exception:
                    SiGui.TextColoured(Colours.Error, Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Exception);
                    SiGui.TextWrapped(Strings.Modules_FriendStreamConnectionModule_ConnectionStatus_Exception_Description);
                    break;
            }
            ImGui.Dummy(Spacing.SectionSpacing);
            SiGui.Heading(Strings.Modules_FriendStreamConnectionModule_ConnectionStatistics);
            SiGui.TextWrapped(string.Format(Strings.Modules_FriendStreamConnectionModule_ConnectionStatistics_EventsTotal, this.EventsRecieved));
            SiGui.TextWrapped(string.Format(Strings.Modules_FriendStreamConnectionModule_ConnectionStatistics_LastEvent, $"{this.LasEventTime:HH:mm:ss}"));
            SiGui.TextWrapped(string.Format(Strings.Modules_FriendStreamConnectionModule_ConnectionStatistics_HeartbeatsTotal, this.HeartbeatsRecieved));
            SiGui.TextWrapped(string.Format(Strings.Modules_FriendStreamConnectionModule_ConnectionStatistics_LastHeartbeat, $"{this.LastHeartbeatTime:HH:mm:ss}"));
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
