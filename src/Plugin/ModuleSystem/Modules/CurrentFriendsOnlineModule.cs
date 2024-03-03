using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Sirensong;
using Sirensong.Cache;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class CurrentFriendsOnlineModule : ModuleBase
    {
        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private CurrentFriendsOnlineModuleConfig Config { get; set; } = ModuleConfigBase.Load<CurrentFriendsOnlineModuleConfig>();

        /// <inheritdoc/>
        public override string Name { get; } = "Friends Online";

        /// <inheritdoc/>
        public override string? Description { get; } = "Get notifications about the friends that are currently online. These notifications work regardless of whether not they have the plugin installed.";

        /// <inheritdoc/>
        public override ModuleTag Tag { get; } = ModuleTag.Notifications;

        /// <inheritdoc/>
        protected override void OnEnable() => DalamudInjections.ClientState.Login += this.OnLogin;

        /// <inheritdoc/>
        protected override void OnDisable() => DalamudInjections.ClientState.Login -= this.OnLogin;

        /// <inheritdoc/>
        protected override void DrawModule()
        {
            SiGui.Heading("Upon Login");

            var friendCountOnlineEnabled = this.Config.ShowOnlineFriendCountOnLogin;
            if (SiGui.Checkbox("Show online friend count in chat", "Sends a message in chat showing how many friends are online upon you logging in. Please note that this may not always display friends currently on other worlds.", ref friendCountOnlineEnabled))
            {
                this.Config.ShowOnlineFriendCountOnLogin = friendCountOnlineEnabled;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            ImGui.BeginDisabled(!friendCountOnlineEnabled);
            ImGui.Indent();
            var namesOnLogin = this.Config.AddFriendNamesToOnlineCount;
            if (SiGui.Checkbox("Also show online friend names", "Add the names of all online friends to the count message.", ref namesOnLogin))
            {
                this.Config.AddFriendNamesToOnlineCount = namesOnLogin;
                this.Config.Save();
            }
            ImGui.Dummy(Spacing.ReadableSpacing);

            var time = (int)this.Config.OnlineFriendShowDelay.TotalSeconds;
            SiGui.Text("Login Delay");
            if (SiGui.SliderInt("##OnlineFriendShowDelaySlider", ref time, CurrentFriendsOnlineModuleConfig.OnlineFriendShowDelayMinSeconds, CurrentFriendsOnlineModuleConfig.OnlineFriendShowDelayMaxSeconds, false))
            {
                this.Config.OnlineFriendShowDelay = TimeSpan.FromSeconds(time);
                this.Config.Save();
            }
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(FontAwesomeIcon.ArrowsSpin))
            {
                this.Config.OnlineFriendShowDelay = TimeSpan.FromSeconds(CurrentFriendsOnlineModuleConfig.OnlineFriendShowDelayDefaultSeconds);
                this.Config.Save();
            }
            SiGui.TextDisabledWrapped("The amount of seconds to wait after logging in to display the message. If online friends are being not shown on login try increasing this value.");
            ImGui.Unindent();
            ImGui.EndDisabled();
        }

        private void OnLogin()
        {
            // Don't show a message if disabled
            if (!this.Config.ShowOnlineFriendCountOnLogin)
            {
                return;
            }

            Task.Run(async () =>
            {
                // Wait for the delay after login.
                await Task.Delay(this.Config.OnlineFriendShowDelay);

                // Don't show a message if the player has no friends added.
                if (FriendHelper.FriendList.Length is 0)
                {
                    DalamudInjections.PluginLog.Debug("Friends list was empty upon logging in");
                    return;
                }

                // Get how many friends are online.
                var onlineFriends = FriendHelper.FriendList.ToArray().Where(x => x.State.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Online));
                var onlineFriendCount = onlineFriends.Count();
                DalamudInjections.PluginLog.Debug($"Player has {onlineFriendCount} friends online right now, sending chat message.");
                if (onlineFriendCount is 0)
                {
                    ChatHelper.Print("You have have no friends online.");
                }
                else
                {
                    if (!this.Config.AddFriendNamesToOnlineCount)
                    {
                        ChatHelper.Print($"You have currently {onlineFriendCount} friends online.");
                        return;
                    }

                    unsafe
                    {
                        var chatMessage = new SeStringBuilder().AddText($"There are currently {onlineFriendCount} friends online:");
                        foreach (var friend in onlineFriends)
                        {
                            var name = MemoryHelper.ReadStringNullTerminated((nint)friend.Name);
                            var currentWorld = SirenCore.GetOrCreateService<LuminaCacheService<World>>().GetRow(friend.CurrentWorld)?.Name.ToString() ?? "Unknown";
                            chatMessage.AddText($"\n - {name}");

                            if (DalamudInjections.ClientState.LocalPlayer?.CurrentWorld.Id != friend.CurrentWorld)
                            {
                                chatMessage
                                    .AddText("  ")
                                    .AddIcon(BitmapFontIcon.Aetheryte)
                                    .AddText(" ")
                                    .AddText(currentWorld);
                            }
                        }
                        ChatHelper.Print(chatMessage.Build());
                    }
                }
            });
        }

        /// <summary>
        ///     Configuration for the current online friends module.
        /// </summary>
        private sealed class CurrentFriendsOnlineModuleConfig : ModuleConfigBase
        {
            /// <inheritdoc />
            public override uint Version { get; protected set; }

            /// <inheritdoc />
            protected override string Identifier { get; set; } = "CurrentFriendsOnlineModule";

            /// <summary>
            ///     whether or not the online friend count notification should be shown.
            /// </summary>
            public bool ShowOnlineFriendCountOnLogin { get; set; } = true;

            /// <summary>
            ///     Whether or not to add the names of each online friend to messages that show the number of online friends.
            /// </summary>
            public bool AddFriendNamesToOnlineCount { get; set; }

            /// <summary>
            ///     The delay period before showing the "friends online" message when logging in.
            /// </summary>
            /// <remarks>
            ///     One delay may not work for everyone as network conditions differ. This value has been set to a reasonable
            ///     default that one may wish to change later.
            ///  </remarks>
            public TimeSpan OnlineFriendShowDelay { get; set; } = TimeSpan.FromSeconds(OnlineFriendShowDelayDefaultSeconds);
            public const int OnlineFriendShowDelayDefaultSeconds = 8;
            public const int OnlineFriendShowDelayMinSeconds = 8;
            public const int OnlineFriendShowDelayMaxSeconds = 30;
        }
    }
}