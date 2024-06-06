using Dalamud.Memory;
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
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;
using Sirensong.Utility;

namespace GoodFriend.Plugin.ModuleSystem.Modules;

internal sealed class WorldChangeModule : BaseModule
{
    /// <summary>
    ///     World lumina sheet for discovering the real name of a world.
    /// </summary>
    private readonly LuminaCacheService<World> worldCache = SirenCore.GetOrCreateService<LuminaCacheService<World>>();

    /// <summary>
    ///     The last world ID of the player.
    /// </summary>
    private uint currentWorldId;

    /// <summary>
    ///     Whether this is the first world update.
    /// </summary>
    private bool firstWorldUpdate = true;

    /// <inheritdoc />
    public override string Name => Strings.Modules_WorldChangeModule_Title;

    /// <inheritdoc />
    public override string? Description => Strings.Modules_WorldChangeModule_Description;

    /// <inheritdoc />
    public override ModuleTag Tag => ModuleTag.Notifications;

    /// <summary>
    ///     The configuration for this module.
    /// </summary>
    private WorldChangeModuleConfig Config { get; } = BaseModuleConfig.Load<WorldChangeModuleConfig>();

    /// <inheritdoc />
    protected override void OnEnable()
    {
        DalamudInjections.Framework.Update += this.OnFrameworkUpdate;
        DalamudInjections.ClientState.Logout += this.OnLogout;
        PlayerEventSseStream.OnStreamMessage += this.OnPlayerStreamMessage;
    }

    /// <inheritdoc />
    protected override void OnDisable()
    {
        DalamudInjections.Framework.Update -= this.OnFrameworkUpdate;
        DalamudInjections.ClientState.Logout -= this.OnLogout;
        PlayerEventSseStream.OnStreamMessage -= this.OnPlayerStreamMessage;
    }

    /// <inheritdoc />
    protected override void DrawModule()
    {
        SiGui.Heading(Strings.Modules_WorldChangeModule_UI_EventSettings);
        var receiveEvents = this.Config.ReceiveEvents;
        if (SiGui.Checkbox(Strings.Modules_WorldChangeModule_UI_EventSettings_ReceieveEvents, Strings.Modules_WorldChangeModule_UI_EventSettings_ReceieveEvents_Description, ref receiveEvents))
        {
            this.Config.ReceiveEvents = receiveEvents;
            this.Config.Save();
        }
        ImGui.Dummy(Spacing.ReadableSpacing);

        // Filtering options
        SiGui.Heading(Strings.Modules_WorldChangeModule_UI_FilteringOptions);
        var onlyShowCurrentWorld = this.Config.OnlyShowCurrentWorld;
        if (SiGui.Checkbox(Strings.Modules_WorldChangeModule_UI_OnlyCurrentWorld, Strings.Modules_WorldChangeModule_UI_OnlyCurrentWorld_Description, ref onlyShowCurrentWorld))
        {
            this.Config.OnlyShowCurrentWorld = onlyShowCurrentWorld;
            this.Config.Save();
        }
        ImGui.Dummy(Spacing.SectionSpacing);

        SiGui.Heading(Strings.Modules_WorldChangeModule_UI_MessageOptions);
        var changeMessage = this.Config.ChangeMessage;
        if (SiGui.InputText(Strings.Modules_LoginStateModule_UI_WorldChangeMessage, ref changeMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            switch (!WorldChangeModuleConfig.ValidateMessage(changeMessage))
            {
                case true:
                    NotificationUtil.ShowErrorToast(Strings.Modules_WorldChangeModule_UI_WorldChangeMessage_Invalid);
                    break;
                case false:
                    this.Config.ChangeMessage = changeMessage;
                    this.Config.Save();
                    break;
            }
        }
        SiGui.AddTooltip(Strings.Modules_WorldChangeModule_UI_WorldChangeMessage_Tooltip);
    }

    /// <summary>
    ///     Called when the player logs out to reset the world ID and first world update.
    /// </summary>
    private void OnLogout()
    {
        this.currentWorldId = 0;
        this.firstWorldUpdate = true;
    }

    /// <summary>
    ///     Called when a player event is received.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="rawEvent"></param>
    private unsafe void OnPlayerStreamMessage(object? _, PlayerEventStreamUpdate rawEvent)
    {
        if (!this.Config.ReceiveEvents)
        {
            return;
        }
        if (!rawEvent.StateUpdateType.WorldChange.HasValue)
        {
            Logger.Verbose("Ignoring world player event as it is not a world change.");
            return;
        }

        DalamudInjections.Framework.Run(() =>
        {
            var localPlayer = DalamudInjections.ClientState.LocalPlayer;
            if (localPlayer is null)
            {
                Logger.Verbose("Ignoring world player event as the local player is null (not logged in?).");
                return;
            }

            // Skip if WorldId is 0 as that's an invalid event.
            var worldChangeData = rawEvent.StateUpdateType.WorldChange.Value;
            if (worldChangeData.WorldId == 0)
            {
                return;
            }

            // Skip if the event did not occur on the same datacenter as the event isn't useful in that case.
            if (this.worldCache.GetRow(worldChangeData.WorldId)?.DataCenter.Row != localPlayer.CurrentWorld.GameData?.DataCenter.Row)
            {
                Logger.Verbose($"Ignoring world player event as it is not from the same datacenter.");
                return;
            }

            // Skip if the event player is not on the players friendslist.
            var friendFromhash = FriendUtil.GetFriendFromHash(rawEvent.ContentIdHash, rawEvent.ContentIdSalt);
            if (!friendFromhash.HasValue)
            {
                Logger.Verbose($"Ignoring world player event as the friend could not be found.");
                return;
            }
            var friendCharacterData = friendFromhash.Value;

            // Skip the event if the friend request is pending.
            if (friendCharacterData.ExtraFlags == Constants.WaitingForFriendListApproval)
            {
                Logger.Debug($"Ignoring world player event for a pending friend request.");
                return;
            }

            // Skip if the player has requested to only see when people travel to their world.
            if (this.Config.OnlyShowCurrentWorld && worldChangeData.WorldId != this.currentWorldId)
            {
                Logger.Debug($"Ignoring world player event as it is not from the current world.");
                return;
            }

            // Show a message as long as world data can be fetched
            var friendName = MemoryHelper.ReadSeStringNullTerminated((nint)friendCharacterData.Name);
            var worldName = this.worldCache.GetRow(worldChangeData.WorldId)?.Name;
            if (worldName is null)
            {
                Logger.Warning($"Could not find world name for world id {worldChangeData.WorldId}.");
                return;
            }
            ChatHelper.Print(this.Config.ChangeMessage.Format(friendName, worldName));
        });
    }

    /// <summary>
    ///     Called when the framework updates to check if the player has changed worlds.
    /// </summary>
    /// <param name="framework"></param>
    private void OnFrameworkUpdate(IFramework framework)
    {
        // Don't run if not logged in.
        if (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer is null)
        {
            return;
        }

        // Set the current world ID if this is the first world update.
        var worldId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.Id;
        if (this.firstWorldUpdate)
        {
            Logger.Debug($"First update, setting last world id to {worldId} and dropping event.");
            this.currentWorldId = worldId;
            this.firstWorldUpdate = false;
            return;
        }

        // Send the world change event if the world ID has changed.
        if (worldId != 0 && this.currentWorldId != worldId)
        {
            Logger.Debug($"World changed from {this.currentWorldId} to {worldId}, sending event.");
            this.currentWorldId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.Id;

            var salt = CryptoUtil.GenerateSalt();
            var hash = CryptoUtil.HashValue(DalamudInjections.ClientState.LocalContentId, salt);
            new PostPlayerWorldChangeRequest().Send(HttpClient, new()
            {
                ContentIdHash = hash,
                ContentIdSalt = salt,
                WorldId = this.currentWorldId,
            });
        }
    }

    /// <summary>
    ///     Configuration for the world change module.
    /// </summary>
    internal sealed class WorldChangeModuleConfig : BaseModuleConfig
    {
        /// <inheritdoc />
        public override uint Version { get; protected set; }

        /// <inheritdoc />
        protected override string Identifier { get; set; } = "WorldChangeModule";

        /// <summary>
        ///     Whether to receive world change events from other players.
        /// </summary>
        public bool ReceiveEvents { get; set; }

        /// <summary>
        ///     Whether to only show when a player travels to the current world.
        /// </summary>
        public bool OnlyShowCurrentWorld { get; set; } = true;

        /// <summary>
        ///     The message to send when a player changes worlds.
        /// </summary>
        public string ChangeMessage { get; set; } = "{0} moved world to {1}.";

        /// <summary>
        ///     Validates a world change message.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <returns>Whether the message is valid.</returns>
        public static bool ValidateMessage(string message) => Validator.TestFormatString(message, 2);
    }
}
