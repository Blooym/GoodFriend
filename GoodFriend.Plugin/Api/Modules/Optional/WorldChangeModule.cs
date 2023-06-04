using System;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Memory;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using Lumina.Excel.GeneratedSheets;
using Sirensong;
using Sirensong.Cache;
using Sirensong.Game.Helpers;

namespace GoodFriend.Plugin.Api.Modules.Optional
{
    internal sealed class WorldChangeModule : ApiOptionalModule
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
        public override string Name => "World Change Notifications";

        /// <inheritdoc />
        public override ApiModuleTag Tag => ApiModuleTag.Friends;

        /// <inheritdoc />
        protected override WorldChangeModuleConfig Config { get; } = ApiModuleConfigBase.Load<WorldChangeModuleConfig>();

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
                return;
            }
            var stateData = rawEvent.StateUpdateType.WorldChange.Value;

            var friendData = ApiFriendUtil.GetFriendByHash(rawEvent.ContentIdHash, rawEvent.ContentIdSalt);
            if (!friendData.HasValue)
            {
                return;
            }

            var friend = friendData.Value;
            var friendName = MemoryHelper.ReadSeStringNullTerminated((nint)friend.Name);
            var world = this.worldCache.GetRow(stateData.WorldId)?.Name;
            if (world == null)
            {
                return;
            }

            ChatHelper.Print($"{friendName} moved world to to {world}.");
        }

        /// <summary>
        ///     Called when the framework updates to check if the player has changed worlds.
        /// </summary>
        /// <param name="framework"></param>
        private void OnFrameworkUpdate(Framework framework)
        {
            if (!DalamudInjections.ClientState.IsLoggedIn || DalamudInjections.ClientState.LocalPlayer == null)
            {
                return;
            }

            var worldId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.Id;
            if (this.firstWorldUpdate)
            {
                Logger.Information($"First update, setting last world id to {worldId} and dropping event.");
                this.currentWorldId = worldId;
                this.firstWorldUpdate = false;
                return;
            }

            if (worldId != 0 && this.currentWorldId != worldId)
            {
                PluginLog.Information($"World changed from {this.currentWorldId} to {worldId}.");
                this.currentWorldId = DalamudInjections.ClientState.LocalPlayer.CurrentWorld.Id;

                var salt = ApiCryptoUtil.GenerateSalt();
                var hash = ApiCryptoUtil.HashValue(DalamudInjections.ClientState.LocalContentId, salt);
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
    internal sealed class WorldChangeModuleConfig : ApiOptionalModuleConfig
    {
        /// <inheritdoc />
        public override uint Version { get; protected set; }

        /// <inheritdoc />
        protected override string Identifier { get; set; } = "WorldChangeModule";

        /// <inheritdoc />
        public override bool Enabled { get; set; }
    }
}