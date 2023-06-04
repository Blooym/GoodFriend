using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Memory;
using Dalamud.Utility;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.Extensions;
using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.Api.Modules.Optional
{
    /// <inheritdoc />
    internal sealed class LoginStateModule : ApiOptionalModule
    {
        /// <summary>
        ///     The current world ID.
        /// </summary>
        private uint currentWorldId;

        /// <summary>
        ///     The current territory ID.
        /// </summary>
        private ushort currentTerritoryId;

        /// <summary>
        ///     The current data center ID.
        /// </summary>
        private uint currentDatacenterId;

        /// <summary>
        ///     The current free company ID.
        /// </summary>
        private uint currentHomeworldId;

        /// <summary>
        ///     The current player content ID.
        /// </summary>
        private ulong currentContentId;

        /// <inheritdoc />
        public override string Name => "Login State Notifications";

        /// <inheritdoc />
        public override ApiModuleTag Tag => ApiModuleTag.Friends;

        /// <inheritdoc />
        protected override LoginStateModuleConfig Config { get; } = ApiModuleConfigBase.Load<LoginStateModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction()
        {
            ApiClient.OnPlayerStreamMessage += this.HandlePlayerStreamMessage;
            DalamudInjections.Framework.Update += this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            ApiClient.OnPlayerStreamMessage -= this.HandlePlayerStreamMessage;
            DalamudInjections.Framework.Update -= this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.ClientState.Logout -= this.OnLogout;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            // Filtering
            SiGui.Heading("Filtering Options");
            var hideSameFc = this.Config.HideSameFC;
            if (SiGui.Checkbox("Hide same free company", "Hides events from friends in the same free company as you.", ref hideSameFc))
            {
                this.Config.HideSameFC = hideSameFc;
                this.Config.Save();
            }

            var hideDifferentHomeworld = this.Config.HideDifferentHomeworld;
            if (SiGui.Checkbox("Hide different homeworld", "Hides events from friends that aren't from your homeworld.", ref hideDifferentHomeworld))
            {
                this.Config.HideDifferentHomeworld = hideDifferentHomeworld;
                this.Config.Save();
            }

            var hideDifferentTerritory = this.Config.HideDifferentTerritory;
            if (SiGui.Checkbox("Hide different territory", "Hides events from friends that logged outside of your current territory.", ref hideDifferentTerritory))
            {
                this.Config.HideDifferentTerritory = hideDifferentTerritory;
                this.Config.Save();
            }

            var hideDifferentWorld = this.Config.HideDifferentWorld;
            if (SiGui.Checkbox("Hide different world", "Hides events from friends that logged outside of your current world.", ref hideDifferentWorld))
            {
                this.Config.HideDifferentWorld = hideDifferentWorld;
                this.Config.Save();
            }

            var hideDifferentDatacenter = this.Config.HideDifferentDatacenter;
            if (SiGui.Checkbox("Hide different data center", "Hides events from friends that logged outside of your current data center.", ref hideDifferentDatacenter))
            {
                this.Config.HideDifferentDatacenter = hideDifferentDatacenter;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.SectionSpacing);

            // Messages
            SiGui.Heading("Message Options");
            var loginMessage = this.Config.LoginMessage;
            if (SiGui.InputText("Login message", ref loginMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (!LoginStateModuleConfig.ValidateMessage(loginMessage))
                {
                    InvalidMessagePopup();
                    return;
                }

                this.Config.LoginMessage = loginMessage.TrimAndSquish();
                this.Config.Save();
            }
            SiGui.AddTooltip("Your message must contain {0} for the player's name.");

            var logoutMessage = this.Config.LogoutMessage;
            if (SiGui.InputText("Logout message", ref logoutMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (!LoginStateModuleConfig.ValidateMessage(logoutMessage))
                {
                    InvalidMessagePopup();
                    return;
                }

                this.Config.LogoutMessage = logoutMessage.TrimAndSquish();
                this.Config.Save();
            }
            SiGui.AddTooltip("Your message must contain {0} for the player's name.");
        }

        /// <summary>
        ///     Called when a player state update is recieved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="rawEvent"></param>
        private unsafe void HandlePlayerStreamMessage(object? sender, PlayerEventStreamUpdate rawEvent)
        {
            // Check if the update is a login/logout.
            if (!rawEvent.StateUpdateType.LoginStateChange.HasValue)
            {
                return;
            }

            // Check if the local player exists.
            var localPlayer = DalamudInjections.ClientState.LocalPlayer;
            if (localPlayer is null)
            {
                return;
            }

            // Check if the local player is the same as the player who logged in/out.
            var friendData = ApiFriendUtil.GetFriendByHash(rawEvent.ContentIdHash, rawEvent.ContentIdSalt);
            if (!friendData.HasValue)
            {
                return;
            }

            // Use the friend's data.
            var friend = friendData.Value;
            var friendName = MemoryHelper.ReadSeStringNullTerminated((nint)friend.Name).TextValue;
            var friendFcTag = MemoryHelper.ReadSeStringNullTerminated((nint)friend.FCTag).TextValue;
            var stateData = rawEvent.StateUpdateType.LoginStateChange.Value;

            Logger.Debug($"Recieved login state update from {friendName}, checking if it should be displayed.");

            // Check if the friend is in the same free company.
            if (this.Config.HideSameFC && friendFcTag != localPlayer.CompanyTag.TextValue)
            {
                Logger.Debug($"Ignoring login state update from {friendName} due to being in the same free company.");
                return;
            }

            // Check if the friend is in the same homeworld.
            if (this.Config.HideDifferentHomeworld && friend.HomeWorld != this.currentHomeworldId)
            {
                Logger.Debug($"Ignoring login state update from different homeworld ({friend.HomeWorld} != {this.currentHomeworldId}).");
                return;
            }

            // Check if the friend is in the same territory.
            if (this.Config.HideDifferentTerritory && stateData.TerritoryId != this.currentTerritoryId)
            {
                Logger.Debug($"Ignoring login state update from different territory ({stateData.TerritoryId} != {this.currentTerritoryId}).");
                return;
            }

            // Check if the friend is in the same world.
            if (this.Config.HideDifferentWorld && stateData.WorldId != this.currentWorldId)
            {
                Logger.Debug($"Ignoring login state update from different world ({stateData.WorldId} != {this.currentWorldId}).");
                return;
            }

            // Check if the friend is in the same data center.
            if (this.Config.HideDifferentDatacenter && stateData.DatacenterId != this.currentDatacenterId)
            {
                Logger.Debug($"Ignoring login state update from different data center ({stateData.DatacenterId} != {this.currentDatacenterId}).");
                return;
            }

            // Send the message.
            ChatHelper.Print(stateData.LoggedIn ? this.Config.LoginMessage.Format(friendName) : this.Config.LogoutMessage.Format(friendName));
        }

        private void OnLogin(object? sender, EventArgs e) =>
                Task.Run(() =>
                {
                    while (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer == null)
                    {
                        Thread.Sleep(100);
                    }

                    this.currentContentId = DalamudInjections.ClientState.LocalContentId;
                    this.currentHomeworldId = DalamudInjections.ClientState.LocalPlayer.HomeWorld.Id;
                    this.currentDatacenterId = DalamudInjections.ClientState.LocalPlayer.HomeWorld.GameData!.DataCenter.Row;

                    Logger.Information("Sending login event.");

                    var salt = ApiCryptoUtil.GenerateSalt();
                    var hash = ApiCryptoUtil.HashValue(this.currentContentId, salt);
                    ApiClient.SendLoginState(new UpdatePlayerLoginStateRequest.PutData()
                    {
                        ContentIdHash = hash,
                        ContentIdSalt = salt,
                        LoggedIn = true,
                        DatacenterId = this.currentDatacenterId,
                        TerritoryId = this.currentTerritoryId,
                        WorldId = this.currentWorldId
                    });
                }).Start();

        private void OnLogout(object? sender, EventArgs e) =>
            Task.Run(() =>
                {
                    this.currentContentId = 0;
                    this.currentHomeworldId = 0;
                    this.currentDatacenterId = 0;
                    this.currentTerritoryId = 0;
                    this.currentWorldId = 0;

                    Logger.Information("Sending logout event.");

                    var salt = ApiCryptoUtil.GenerateSalt();
                    var hash = ApiCryptoUtil.HashValue(this.currentContentId, salt);
                    ApiClient.SendLoginState(new UpdatePlayerLoginStateRequest.PutData()
                    {
                        ContentIdHash = hash,
                        ContentIdSalt = salt,
                        LoggedIn = false,
                        DatacenterId = this.currentDatacenterId,
                        TerritoryId = this.currentTerritoryId,
                        WorldId = this.currentWorldId
                    });
                }).Start();

        /// <summary>
        ///     Called when the framework updates, updates stored data that can change during gameplay.
        /// </summary>
        /// <param name="framework"></param>
        private void OnFrameworkUpdate(Framework framework)
        {
            if (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer == null)
            {
                return;
            }

            var currentWorld = DalamudInjections.ClientState.LocalPlayer.CurrentWorld;
            if (currentWorld != null && currentWorld.Id != 0 && currentWorld.Id != this.currentWorldId)
            {
                this.currentWorldId = currentWorld.Id;
            }

            var currentTerritory = DalamudInjections.ClientState.TerritoryType;
            if (currentTerritory != 0 && currentTerritory != this.currentTerritoryId)
            {
                this.currentTerritoryId = currentTerritory;
            }

            var currentDatacenterId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.GameData?.DataCenter.Row ?? 0;
            if (currentDatacenterId != 0 && currentDatacenterId != this.currentDatacenterId)
            {
                this.currentDatacenterId = currentDatacenterId;
            }
        }

        /// <summary>
        ///     Called when the user enters an invalid message.
        /// </summary>
        private static void InvalidMessagePopup()
        {
            ToastHelper.ShowErrorToast("The message you entered is invalid.");
            SoundEffectHelper.PlaySound(SoundEffect.Se11);
        }

        /// <inheritdoc />
        internal sealed class LoginStateModuleConfig : ApiOptionalModuleConfig
        {
            /// <inheritdoc />
            public override uint Version { get; protected set; }

            /// <inheritdoc />
            protected override string Identifier { get; set; } = "LoginStateModule";

            /// <inheritdoc />
            public override bool Enabled { get; set; } = true;

            /// <summary>
            ///     Whether or not to hide notifications for the same free company.
            /// </summary>
            public bool HideSameFC { get; set; } = true;

            /// <summary>
            ///     Whether or not to hide notifications from users from different homeworlds.
            /// </summary>
            public bool HideDifferentHomeworld { get; set; }

            /// <summary>
            ///     Whether or not to hide notifications from users in different territories.
            /// </summary>
            public bool HideDifferentTerritory { get; set; }

            /// <summary>
            ///     Whether or not to hide notifications from users in different worlds.
            /// </summary>
            public bool HideDifferentWorld { get; set; }

            /// <summary>
            ///     Whether or not to hide notifications from users in different data centers.
            /// </summary>
            public bool HideDifferentDatacenter { get; set; }

            /// <summary>
            ///     The message to display when a friend logs in.
            /// </summary>
            public string LoginMessage { get; set; } = "{0} has logged in.";

            /// <summary>
            ///     The message to display when a friend logs out.
            /// </summary>
            public string LogoutMessage { get; set; } = "{0} has logged out.";

            /// <summary>
            ///     Validates a login/logout message.
            /// </summary>
            /// <param name="message">The message to validate.</param>
            /// <returns>Whether or not the message is valid.</returns>
            public static bool ValidateMessage(string message) => message.Contains("{0}");
        }
    }
}