using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Memory;
using Dalamud.Utility;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.Utility;
using ImGuiNET;
using Sirensong.Extensions;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;
using Sirensong.Utility;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    /// <inheritdoc />
    internal sealed class LoginStateModule : ModuleBase
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
        public override string Name => Strings.Modules_LoginStateModule_Name;

        /// <inheritdoc />
        public override string? Description => Strings.Modules_LoginStateModule_Description;

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Notifications;

        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private LoginStateModuleConfig Config { get; } = ModuleConfigBase.Load<LoginStateModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction()
        {
            ApiClient.OnPlayerStreamMessage += this.HandlePlayerStreamMessage;
            DalamudInjections.Framework.Update += this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Login += this.OnLogin;
            DalamudInjections.ClientState.Logout += this.OnLogout;

            if (DalamudInjections.ClientState.IsLoggedIn && DalamudInjections.ClientState.LocalPlayer is not null)
            {
                this.SetStoredValues();
            }
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            ApiClient.OnPlayerStreamMessage -= this.HandlePlayerStreamMessage;
            DalamudInjections.Framework.Update -= this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.ClientState.Logout -= this.OnLogout;

            this.ClearStoredValues();
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
            // Event settings
            SiGui.Heading(Strings.Modules_LoginStateModule_UI_EventSettings);
            var receiveEvents = this.Config.ReceiveEvents;
            if (SiGui.Checkbox(Strings.Modules_LoginStateModule_UI_EventSettings_ReceieveEvents, Strings.Modules_LoginStateModule_UI_EventSettings_ReceieveEvents_Description, ref receiveEvents))
            {
                this.Config.ReceiveEvents = receiveEvents;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            // Filtering
            SiGui.Heading(Strings.Modules_LoginStateModule_UI_FilteringOptions);
            var hideSameFc = this.Config.HideSameFC;
            if (SiGui.Checkbox(Strings.Modules_LoginStateModule_UI_HideSameCompany, Strings.Modules_LoginStateModule_UI_HideSameCompany_Description, ref hideSameFc))
            {
                this.Config.HideSameFC = hideSameFc;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            var hideDifferentHomeworld = this.Config.HideDifferentHomeworld;
            if (SiGui.Checkbox(Strings.Modules_LoginStateModule_UI_HideDiffHomeworld, Strings.Modules_LoginStateModule_UI_HideDiffHomeworld_Description, ref hideDifferentHomeworld))
            {
                this.Config.HideDifferentHomeworld = hideDifferentHomeworld;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            var hideDifferentTerritory = this.Config.HideDifferentTerritory;
            if (SiGui.Checkbox(Strings.Modules_LoginStateModule_UI_HideDiffTerritory, Strings.Modules_LoginStateModule_UI_HideDiffTerritory_Description, ref hideDifferentTerritory))
            {
                this.Config.HideDifferentTerritory = hideDifferentTerritory;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            var hideDifferentWorld = this.Config.HideDifferentWorld;
            if (SiGui.Checkbox(Strings.Modules_LoginStateModule_UI_HideDiffWorld, Strings.Modules_LoginStateModule_UI_HideDiffWorld_Description, ref hideDifferentWorld))
            {
                this.Config.HideDifferentWorld = hideDifferentWorld;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            var hideDifferentDatacenter = this.Config.HideDifferentDatacenter;
            if (SiGui.Checkbox(Strings.Modules_LoginStateModule_UI_HideDiffDC, Strings.Modules_LoginStateModule_UI_HideDiffDC_Description, ref hideDifferentDatacenter))
            {
                this.Config.HideDifferentDatacenter = hideDifferentDatacenter;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.SectionSpacing);

            // Messages
            SiGui.Heading(Strings.Modules_LoginStateModule_UI_MessageOptions);
            var loginMessage = this.Config.LoginMessage;
            if (SiGui.InputText(Strings.Modules_LoginStateModule_UI_LoginMessage, ref loginMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                switch (LoginStateModuleConfig.ValidateMessage(loginMessage))
                {
                    case true:
                        this.Config.LoginMessage = loginMessage.TrimAndSquish();
                        this.Config.Save();
                        break;
                    case false:
                        NotificationUtil.ShowErrorToast(Strings.Modules_LoginStateModule_UI_LoginMessage_Invalid);
                        break;
                }
            }
            SiGui.AddTooltip(Strings.Modules_LoginStateModule_UI_LoginMessage_Tooltip);

            var logoutMessage = this.Config.LogoutMessage;
            if (SiGui.InputText(Strings.Modules_LoginStateModule_UI_LogoutMessage, ref logoutMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                switch (LoginStateModuleConfig.ValidateMessage(logoutMessage))
                {
                    case true:
                        this.Config.LogoutMessage = logoutMessage.TrimAndSquish();
                        this.Config.Save();
                        break;
                    case false:
                        NotificationUtil.ShowErrorToast(Strings.Modules_LoginStateModule_UI_LogoutMessage_Invalid);
                        break;
                }
            }
            SiGui.AddTooltip(Strings.Modules_LoginStateModule_UI_LogoutMessage_Tooltip);
        }

        /// <summary>
        ///     Called when a player state update is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="rawEvent"></param>
        private unsafe void HandlePlayerStreamMessage(object? sender, PlayerEventStreamUpdate rawEvent)
        {
            // Ignore the event if recieving events is disabled.
            if (!this.Config.ReceiveEvents)
            {
                return;
            }

            // Check if the update is a login/logout.
            if (!rawEvent.StateUpdateType.LoginStateChange.HasValue)
            {
                Logger.Verbose("Received player state update that is not a login/logout.");
                return;
            }

            // Check if the local player exists.
            var localPlayer = DalamudInjections.ClientState.LocalPlayer;
            if (localPlayer is null)
            {
                Logger.Verbose("Ignoring player state update as the local player is null.");
                return;
            }

            // Check if the local player is the same as the player who logged in/out.
            var friendData = FriendUtil.GetFriendByHash(rawEvent.ContentIdHash, rawEvent.ContentIdSalt);
            if (!friendData.HasValue)
            {
                Logger.Verbose($"Ignoring player event as the friend could not be found.");
                return;
            }

            // Use the friend's data.
            var friend = friendData.Value;
            var friendName = MemoryHelper.ReadSeStringNullTerminated((nint)friend.Name).TextValue;
            var friendFcTag = MemoryHelper.ReadSeStringNullTerminated((nint)friend.FCTag).TextValue;
            var stateData = rawEvent.StateUpdateType.LoginStateChange.Value;

            Logger.Debug($"Received login state update from {friendName}, checking if it should be displayed.");

            // Check if the friend is in the same free company.
            if (this.Config.HideSameFC && friendFcTag == localPlayer.CompanyTag.TextValue)
            {
                Logger.Debug($"Ignoring login state update from {friendName} due to being in the same free company ({friendFcTag} == {localPlayer.CompanyTag.TextValue}).");
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

        /// <summary>
        ///     Called when the player logs in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogin(object? sender, EventArgs e) =>
                Task.Run(() =>
                {
                    while (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer is null)
                    {
                        Thread.Sleep(100);
                    }

                    this.SetStoredValues();

                    Logger.Debug("Sending login event.");

                    var salt = CryptoUtil.GenerateSalt();
                    var hash = CryptoUtil.HashWithSalt(this.currentContentId, salt);
                    ApiClient.SendLoginState(new UpdatePlayerLoginStateRequest.PutData()
                    {
                        ContentIdHash = hash,
                        ContentIdSalt = salt,
                        LoggedIn = true,
                        DatacenterId = this.currentDatacenterId,
                        TerritoryId = this.currentTerritoryId,
                        WorldId = this.currentWorldId
                    });
                });

        /// <summary>
        ///     Called when the player logs out, sends a logout event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogout(object? sender, EventArgs e) =>
            Task.Run(() =>
                {
                    var salt = CryptoUtil.GenerateSalt();
                    var hash = CryptoUtil.HashWithSalt(this.currentContentId, salt);
                    Logger.Debug("Sending logout event.");
                    ApiClient.SendLoginState(new UpdatePlayerLoginStateRequest.PutData()
                    {
                        ContentIdHash = hash,
                        ContentIdSalt = salt,
                        LoggedIn = false,
                        DatacenterId = this.currentDatacenterId,
                        TerritoryId = this.currentTerritoryId,
                        WorldId = this.currentWorldId
                    });

                    this.ClearStoredValues();
                });

        /// <summary>
        ///     Called when the framework updates, updates stored data that can change during gameplay.
        /// </summary>
        /// <param name="framework"></param>
        private void OnFrameworkUpdate(Framework framework)
        {
            // Don't run when not logged in.
            if (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer is null)
            {
                return;
            }

            // Update current world.
            var currentWorld = DalamudInjections.ClientState.LocalPlayer?.CurrentWorld;
            if (currentWorld is not null && currentWorld.Id != 0 && currentWorld.Id != this.currentWorldId)
            {
                this.currentWorldId = currentWorld.Id;
            }

            // Update current territory.
            var currentTerritory = DalamudInjections.ClientState.TerritoryType;
            if (currentTerritory != 0 && currentTerritory != this.currentTerritoryId)
            {
                this.currentTerritoryId = currentTerritory;
            }
        }

        /// <summary>
        ///     Sets stored values to their current values, will error if the local player is null.
        /// </summary>
        private void SetStoredValues()
        {
            this.currentContentId = DalamudInjections.ClientState.LocalContentId;
            this.currentHomeworldId = DalamudInjections.ClientState.LocalPlayer!.HomeWorld.Id;
            this.currentDatacenterId = DalamudInjections.ClientState.LocalPlayer.HomeWorld.GameData!.DataCenter.Row;
            this.currentTerritoryId = DalamudInjections.ClientState.TerritoryType;
            this.currentWorldId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.GameData!.RowId;

            Logger.Debug($"Set stored values: CID: XXX, HW: {this.currentHomeworldId}, DC: {this.currentDatacenterId}, T: {this.currentTerritoryId}, W: {this.currentWorldId}");
        }

        /// <summary>
        ///     Clears all stored values.
        /// </summary>
        private void ClearStoredValues()
        {
            this.currentContentId = 0;
            this.currentHomeworldId = 0;
            this.currentDatacenterId = 0;
            this.currentTerritoryId = 0;
            this.currentWorldId = 0;

            Logger.Debug("Cleared stored all values.");
        }
    }

    /// <inheritdoc />
    internal sealed class LoginStateModuleConfig : ModuleConfigBase
    {
        /// <inheritdoc />
        public override uint Version { get; protected set; }

        /// <inheritdoc />
        protected override string Identifier { get; set; } = "LoginStateModule";

        /// <summary>
        ///     Whether or not to receive login state events.
        /// </summary>
        public bool ReceiveEvents { get; set; } = true;

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
        public static bool ValidateMessage(string message) => Validator.TestFormatString(message, 1, true);
    }
}
