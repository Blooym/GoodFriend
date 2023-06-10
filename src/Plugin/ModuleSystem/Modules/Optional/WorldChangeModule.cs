using System;
using Dalamud.Game;
using Dalamud.Memory;
using Dalamud.Utility;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
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

namespace GoodFriend.Plugin.ModuleSystem.Modules.Optional
{
    internal sealed class WorldChangeModule : OptionalModuleBase
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

        /// <inheritdoc />
        protected override WorldChangeModuleConfig Config { get; } = ModuleConfigBase.Load<WorldChangeModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction()
        {
            DalamudInjections.Framework.Update += this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Logout += this.OnLogout;
            ApiClient.OnPlayerStreamMessage += this.OnPlayerStreamMessage;
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            DalamudInjections.Framework.Update -= this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Logout -= this.OnLogout;
            ApiClient.OnPlayerStreamMessage -= this.OnPlayerStreamMessage;
        }

        /// <inheritdoc />
        protected override void DrawModule()
        {
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogout(object? sender, EventArgs e)
        {
            this.currentWorldId = 0;
            this.firstWorldUpdate = true;
        }

        /// <summary>
        ///     Called when a player event is recieved.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rawEvent"></param>
        private unsafe void OnPlayerStreamMessage(object? _, PlayerEventStreamUpdate rawEvent)
        {
            if (!rawEvent.StateUpdateType.WorldChange.HasValue)
            {
                Logger.Verbose("Ignoring player event as it is not a world change.");
                return;
            }
            var stateData = rawEvent.StateUpdateType.WorldChange.Value;

            // Ignore the event if it does not come from the current world if enabled.
            if (this.Config.OnlyShowCurrentWorld && stateData.WorldId != this.currentWorldId)
            {
                Logger.Verbose($"Ignoring player event as it is not from the current world (current: {stateData.WorldId} != {this.currentWorldId}).");
                return;
            }

            // Find the friend that changed worlds, if not found then ignore.
            var friendData = FriendUtil.GetFriendByHash(rawEvent.ContentIdHash, rawEvent.ContentIdSalt);
            if (!friendData.HasValue)
            {
                Logger.Verbose($"Ignoring player event as the friend could not be found.");
                return;
            }

            // Find the world name, if not found then ignore.
            var friend = friendData.Value;
            var friendName = MemoryHelper.ReadSeStringNullTerminated((nint)friend.Name);
            var world = this.worldCache.GetRow(stateData.WorldId)?.Name;
            if (world == null)
            {
                Logger.Warning($"Could not perform lookup to find world name for world id {stateData.WorldId}.");
                return;
            }

            // Print the message.
            ChatHelper.Print(this.Config.ChangeMessage.Format(friendName, world));
        }

        /// <summary>
        ///     Called when the framework updates to check if the player has changed worlds.
        /// </summary>
        /// <param name="framework"></param>
        private void OnFrameworkUpdate(Framework framework)
        {
            // Don't run if not logged in.
            if (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer == null)
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
                ApiClient.SendWorldChangeAsync(new UpdatePlayerWorldRequest.PutData()
                {
                    ContentIdHash = hash,
                    ContentIdSalt = salt,
                    WorldId = this.currentWorldId,
                });
            }
        }
    }

    /// <summary>
    ///     Configuration for the world change module.
    /// </summary>
    internal sealed class WorldChangeModuleConfig : OptionalModuleConfigBase
    {
        /// <inheritdoc />
        public override uint Version { get; protected set; }

        /// <inheritdoc />
        protected override string Identifier { get; set; } = "WorldChangeModule";

        /// <inheritdoc />
        public override bool Enabled { get; set; }

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
        /// <returns>Whether or not the message is valid.</returns>
        public static bool ValidateMessage(string message) => ValidatorUtil.TestFormatString(message, 2, true);
    }
}