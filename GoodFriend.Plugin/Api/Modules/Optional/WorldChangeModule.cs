using System;
using Dalamud.Game;
using Dalamud.Logging;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using Sirensong.Game.Helpers;

namespace GoodFriend.Plugin.Api.Modules.Optional
{
    internal sealed class WorldChangeModule : ApiOptionalModule
    {
        /// <summary>
        ///     The last world ID of the player.
        /// </summary>
        private uint currentWorldId;

        /// <summary>
        ///     Whether this is the first world update.
        /// </summary>
        private bool firstWorldUpdate = true;

        /// <inheritdoc />
        public override string Name => "World Change";

        /// <inheritdoc />
        public override ApiModuleTag Tag => ApiModuleTag.Social;

        /// <inheritdoc />
        protected override WorldChangeModuleConfig Config { get; } = ApiModuleConfigBase.Load<WorldChangeModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction()
        {
            DalamudInjections.Framework.Update += this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Logout += this.OnLogout;
            ApiClient.OnPlayerStreamMessage += OnPlayerStreamMessage;
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            DalamudInjections.Framework.Update -= this.OnFrameworkUpdate;
            DalamudInjections.ClientState.Logout -= this.OnLogout;
            ApiClient.OnPlayerStreamMessage -= OnPlayerStreamMessage;
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
        /// <param name="data"></param>
        private static void OnPlayerStreamMessage(object? _, PlayerEventStreamUpdate data)
        {
            if (data.StateUpdateType.WorldChange == null)
            {
                Logger.Verbose("Ignoring player state update with no world change information.");
                return;
            }

            ChatHelper.Print($"{data.ContentIdHash} changed to {data.StateUpdateType.WorldChange.Value.WorldId}");
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