using System.Threading;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using GoodFriend.Client.Http.Requests;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Sirensong;
using Sirensong.Cache;
using Sirensong.Extensions;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;
using Sirensong.Utility;

namespace GoodFriend.Plugin.ModuleSystem.Modules;

/// <inheritdoc />
internal sealed class LoginStateModule : BaseModule
{
    private readonly LuminaCacheService<World> worldCache = SirenCore.GetOrCreateService<LuminaCacheService<World>>();

    /// <summary>
    ///     The current world ID.
    /// </summary>
    private uint currentWorldId;

    /// <summary>
    ///     The current territory ID.
    /// </summary>
    private ushort currentTerritoryId;

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
    private LoginStateModuleConfig Config { get; } = BaseModuleConfig.Load<LoginStateModuleConfig>();

    /// <inheritdoc />
    protected override void OnEnable()
    {
        PlayerEventSseStream.OnStreamMessage += this.HandlePlayerStreamMessage;
        DalamudInjections.Framework.Update += this.OnFrameworkUpdate;
        DalamudInjections.ClientState.Login += this.OnLogin;
        DalamudInjections.ClientState.Logout += this.OnLogout;

        if (DalamudInjections.ClientState.IsLoggedIn && DalamudInjections.ClientState.LocalPlayer is not null)
        {
            this.SetStoredValues();
        }
    }

    /// <inheritdoc />
    protected override void OnDisable()
    {
        PlayerEventSseStream.OnStreamMessage -= this.HandlePlayerStreamMessage;
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
        if (!this.Config.ReceiveEvents)
        {
            return;
        }
        if (!rawEvent.StateUpdateType.LoginStateChange.HasValue)
        {
            Logger.Verbose("Received player state update that is not a login/logout.");
            return;
        }
        DalamudInjections.Framework.Run(() =>
        {
            var localPlayer = DalamudInjections.ClientState.LocalPlayer;
            if (localPlayer is null)
            {
                Logger.Verbose("Ignoring player state update as the local player is null (not logged in?).");
                return;
            }

            // Skip if the event player is not on the players friendslist.
            var friendFromHash = FriendUtil.GetFriendFromHash(rawEvent.ContentIdHash, rawEvent.ContentIdSalt);
            if (!friendFromHash.HasValue)
            {
                Logger.Verbose(message: $"Ignoring player event as a friend could not be found from the received hash.");
                return;
            }
            var friendCharacterData = friendFromHash.Value;

            // Ignore the event if the friend request is pending.
            if (friendCharacterData.ExtraFlags == Constants.WaitingForFriendListApproval)
            {
                Logger.Debug($"Ignoring login state event for a pending friend request.");
                return;
            }

            // Get the name and free company tag.
            var loginStateData = rawEvent.StateUpdateType.LoginStateChange.Value;
            Logger.Debug($"Received login state update from {friendCharacterData.NameString} - checking display eligibility.");

            // Evaluate eligibility.
            if (this.Config.HideSameFC && friendCharacterData.FCTagString == localPlayer.CompanyTag.TextValue)
            {
                Logger.Debug($"Ignoring login state update from from the same free company.");
                return;
            }
            if (this.Config.HideDifferentHomeworld && friendCharacterData.HomeWorld != this.currentHomeworldId)
            {
                Logger.Debug($"Ignoring login state update from different homeworld.");
                return;
            }
            if (this.Config.HideDifferentTerritory && loginStateData.TerritoryId != this.currentTerritoryId)
            {
                Logger.Debug($"Ignoring login state update from different territory.");
                return;
            }
            if (this.Config.HideDifferentWorld && loginStateData.WorldId != this.currentWorldId)
            {
                Logger.Debug($"Ignoring login state update from different world.");
                return;
            }
            if (this.Config.HideDifferentDatacenter && this.worldCache.GetRow(loginStateData.WorldId)?.DataCenter.Row != localPlayer.CurrentWorld.GameData?.DataCenter.Row)
            {
                Logger.Debug($"Ignoring login state update from different data center.");
                return;
            }

            ChatHelper.Print(loginStateData.LoggedIn
                ? this.Config.LoginMessage.Format(friendCharacterData.NameString)
                : this.Config.LogoutMessage.Format(friendCharacterData.NameString)
            );
        });
    }

    /// <summary>
    ///     Called when the player logs in.
    /// </summary>
    private void OnLogin()
    {
        while (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer is null)
        {
            Thread.Sleep(50);
        }

        this.SetStoredValues();

        Logger.Debug("Sending login event.");

        var salt = CryptoUtil.GenerateSalt();
        var hash = CryptoUtil.HashValue(this.currentContentId, salt);
        new PostPlayerLoginStateRequest().Send(HttpClient, new()
        {
            ContentIdHash = hash,
            ContentIdSalt = salt,
            LoggedIn = true,
            TerritoryId = this.currentTerritoryId,
            WorldId = this.currentWorldId,
        });
    }

    /// <summary>
    ///     Called when the player logs out, sends a logout event.
    /// </summary>
    private void OnLogout()
    {
        var salt = CryptoUtil.GenerateSalt();
        var hash = CryptoUtil.HashValue(this.currentContentId, salt);
        Logger.Debug("Sending logout event.");
        new PostPlayerLoginStateRequest().Send(HttpClient, new()
        {
            ContentIdHash = hash,
            ContentIdSalt = salt,
            LoggedIn = false,
            TerritoryId = this.currentTerritoryId,
            WorldId = this.currentWorldId,
        });
        this.ClearStoredValues();
    }

    /// <summary>
    ///     Called when the framework updates, updates stored data that can change during gameplay.
    /// </summary>
    /// <param name="framework"></param>
    private void OnFrameworkUpdate(IFramework framework)
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
        this.currentTerritoryId = DalamudInjections.ClientState.TerritoryType;
        this.currentWorldId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.GameData!.RowId;

        Logger.Debug($"Set stored values: CID: {this.currentContentId}, HW: {this.currentHomeworldId}, T: {this.currentTerritoryId}, W: {this.currentWorldId}");
    }

    /// <summary>
    ///     Clears all stored values.
    /// </summary>
    private void ClearStoredValues()
    {
        this.currentContentId = 0;
        this.currentHomeworldId = 0;
        this.currentTerritoryId = 0;
        this.currentWorldId = 0;

        Logger.Debug("Cleared stored all values.");
    }

    /// <inheritdoc />
    private sealed class LoginStateModuleConfig : BaseModuleConfig
    {
        /// <inheritdoc />
        public override uint Version { get; protected set; }

        /// <inheritdoc />
        protected override string Identifier { get; set; } = "LoginStateModule";

        /// <summary>
        ///     Whether to receive login state events.
        /// </summary>
        public bool ReceiveEvents { get; set; } = true;

        /// <summary>
        ///     Whether to hide notifications for the same free company.
        /// </summary>
        public bool HideSameFC { get; set; } = true;

        /// <summary>
        ///     Whether to hide notifications from users from different homeworlds.
        /// </summary>
        public bool HideDifferentHomeworld { get; set; }

        /// <summary>
        ///     Whether to hide notifications from users in different territories.
        /// </summary>
        public bool HideDifferentTerritory { get; set; }

        /// <summary>
        ///     Whether to hide notifications from users in different worlds.
        /// </summary>
        public bool HideDifferentWorld { get; set; }

        /// <summary>
        ///     Whether to hide notifications from users in different data centers.
        /// </summary>
        public bool HideDifferentDatacenter { get; set; } = true;

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
        /// <returns>Whether the message is valid.</returns>
        public static bool ValidateMessage(string message) => Validator.TestFormatString(message, 1);
    }
}
