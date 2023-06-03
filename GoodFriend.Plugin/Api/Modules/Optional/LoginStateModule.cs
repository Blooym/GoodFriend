using Dalamud.Memory;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.UserInterface;

namespace GoodFriend.Plugin.Api.Modules.Optional
{
    /// <inheritdoc />
    internal sealed class LoginStateModule : ApiOptionalModule
    {
        /// <inheritdoc />
        public override string Name => "Login State";

        /// <inheritdoc />
        public override ApiModuleTag Tag => ApiModuleTag.Social;

        /// <inheritdoc />
        protected override LoginStateModuleConfig Config { get; } = ApiModuleConfigBase.Load<LoginStateModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction() => ApiClient.OnPlayerStreamMessage += this.HandlePlayerStreamMessage;

        /// <inheritdoc />
        protected override void DisableAction() => ApiClient.OnPlayerStreamMessage -= this.HandlePlayerStreamMessage;

        /// <inheritdoc />
        protected override void DrawModule()
        {
            var hideSameFc = this.Config.HideSameFC;
            var hideDifferentHomeworld = this.Config.HideDifferentHomeworld;
            var hideDifferentTerritory = this.Config.HideDifferentTerritory;
            var hideDifferentWorld = this.Config.HideDifferentWorld;
            var hideDifferentDatacenter = this.Config.HideDifferentDatacenter;
            var loginMessage = this.Config.LoginMessage;
            var logoutMessage = this.Config.LogoutMessage;

            SiGui.Heading("Filtering Options");
            if (SiGui.Checkbox("Hide same free company", ref hideSameFc))
            {
                this.Config.HideSameFC = hideSameFc;
                this.Config.Save();
            }

            if (SiGui.Checkbox("Hide different homeworld", ref hideDifferentHomeworld))
            {
                this.Config.HideDifferentHomeworld = hideDifferentHomeworld;
                this.Config.Save();
            }

            if (SiGui.Checkbox("Hide different territory", ref hideDifferentTerritory))
            {
                this.Config.HideDifferentTerritory = hideDifferentTerritory;
                this.Config.Save();
            }

            if (SiGui.Checkbox("Hide different world", ref hideDifferentWorld))
            {
                this.Config.HideDifferentWorld = hideDifferentWorld;
                this.Config.Save();
            }

            if (SiGui.Checkbox("Hide different data center", ref hideDifferentDatacenter))
            {
                this.Config.HideDifferentDatacenter = hideDifferentDatacenter;
                this.Config.Save();
            }

            if (SiGui.InputText("Login message", ref loginMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.Config.LoginMessage = loginMessage;
                this.Config.Save();
            }

            if (SiGui.InputText("Logout message", ref logoutMessage, 256, true, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.Config.LogoutMessage = logoutMessage;
                this.Config.Save();
            }
        }

        /// <summary>
        ///     Called when a player state update is recieved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private unsafe void HandlePlayerStreamMessage(object? sender, PlayerEventStreamUpdate data)
        {
            if (data.StateUpdateType.LoginStateChange is null)
            {
                Logger.Verbose("Ignoring state update with no login state change information.");
                return;
            }

            var eventFriend = ApiUtil.GetFriendByContentIdHash(data.ContentIdHash, data.ContentIdSalt);
            if (eventFriend is null)
            {
                Logger.Verbose("Ignoring state update from non-friend.");
                return;
            }
            var friendVal = eventFriend.Value;

            Logger.Information($"Recieved state update from {MemoryHelper.ReadSeStringNullTerminated((nint)friendVal.Name)}");
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
        }
    }
}