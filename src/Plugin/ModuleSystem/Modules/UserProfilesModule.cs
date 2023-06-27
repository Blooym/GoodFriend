using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Utility;
using ImGuiNET;
using Sirensong.UserInterface;

// TODO: Add remote profiles synced from database from API.

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class UserProfilesModule : ModuleBase, IDisposable
    {
        /// <summary>
        ///     All allowed agents for view profile.
        /// </summary>
        private static readonly string[] EditNoteMenuAllowedAgents = {
            "FriendList",
            "FreeCompany",
            "SocialList",
            "PartyMemberList",
            "CrossWorldLinkshell",
            "Linkshell"
        };

        /// <summary>
        ///     The context menu instance.
        /// </summary>  
        private DalamudContextMenu contextMenu = null!;

        /// <summary>
        ///     The context menu item for "View Profile"
        /// </summary>
        private GameObjectContextMenuItem playerNoteMenuItem = null!;

        /// <summary>
        ///     The window to view player notes.
        /// </summary>
        private PlayerNoteWindow playerNoteWindow = null!;

        /// <inheritdoc />
        public override string Name => "User Profiles";

        /// <inheritdoc />
        public override string? Description => "User profiles, local notes and more [EXPERIMENTAL]";

        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private UserProfilesModuleConfig Config { get; } = ModuleConfigBase.Load<UserProfilesModuleConfig>();

        /// <inheritdoc />
        public override ModuleTag Tag { get; } = ModuleTag.Experimental;

        /// <inheritdoc />
        protected override void EnableAction()
        {
            this.playerNoteWindow = new(this.Config);
            Services.WindowingService.AddToWindowSystem(this.playerNoteWindow);

            this.contextMenu = new();
            this.playerNoteMenuItem = new GameObjectContextMenuItem(new SeString(new TextPayload("View Player Note")), this.OnPlayerNoteContextMenuPressed, false);
            this.contextMenu.OnOpenGameObjectContextMenu += this.OnPlayerNoteContextMenuAvailable;
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            this.contextMenu.OnOpenGameObjectContextMenu -= this.OnPlayerNoteContextMenuAvailable;
            this.contextMenu.Dispose();

            Services.WindowingService.RemoveFromWindowSystem(this.playerNoteWindow);
        }

        /// <inheritdoc />
        public void Dispose() => this.Disable();

        /// <summary>
        ///     Called when the <see cref="playerNoteMenuItem" /> could be shown.
        /// </summary>
        /// <param name="args"></param>
        private void OnPlayerNoteContextMenuAvailable(GameObjectContextMenuOpenArgs args)
        {
            Logger.Debug($"ParentAddon: {args.ParentAddonName}");
            if (EditNoteMenuAllowedAgents.Contains(args.ParentAddonName))
            {
                args.AddCustomItem(this.playerNoteMenuItem);
            }
        }

        /// <summary>
        ///     Called when <see cref="playerNoteMenuItem" /> is pressed.
        /// </summary>
        /// <param name="args"></param>
        private void OnPlayerNoteContextMenuPressed(GameObjectContextMenuItemSelectedArgs args) => this.playerNoteWindow.Open(args);

        /// <summary>
        ///     The player note window
        /// </summary>
        private sealed class PlayerNoteWindow : Window
        {
            private GameObjectContextMenuItemSelectedArgs? args;
            private readonly UserProfilesModuleConfig config;
            private bool editing;

            /// <inheritdoc />
            public PlayerNoteWindow(UserProfilesModuleConfig config) : base("Player Note")
            {
                this.Size = new(400, 300);
                this.SizeCondition = ImGuiCond.FirstUseEver;
                this.SizeConstraints = new WindowSizeConstraints()
                {
                    MinimumSize = new(300, 200),
                    MaximumSize = new(1000, 1000),
                };
                this.config = config;
            }

            public void Open(GameObjectContextMenuItemSelectedArgs args)
            {
                this.args = args;
                this.IsOpen = true;
            }

            /// <inheritdoc />
            public override void OnClose()
            {
                this.editing = false;
                this.args = null;
            }

            /// <inheritdoc />
            public override void Draw()
            {
                // Safeguard for no player selected.
                if (this.args is null)
                {
                    SiGui.TextWrapped("Player not selected, please select a player and try again,");
                    return;
                }

                // Safeguard for agent giving us a 0 content id or no name.
                var name = this.args.Text?.TextValue;
                var contentId = this.args.ContentIdLower;
                if (contentId == 0 || string.IsNullOrEmpty(name))
                {
                    SiGui.TextWrapped("Unable to find player selected.");
                    return;
                }

                SiGui.Heading(name ?? string.Empty);

                // Get profile and assign it if not found.
                var fContentId = CryptoUtil.Hash(contentId);
                var fProfile = this.config.LocalProfiles.GetValueOrDefault(fContentId, new());
                this.config.LocalProfiles[fContentId] = fProfile;

                var note = fProfile.Note;
                if (ImGui.BeginChild("playerNoteContent", new(0, ImGui.GetContentRegionAvail().Y - 30)))
                {
                    // Editor
                    if (this.editing)
                    {
                        if (SiGui.InputTextMultiline($"##{fContentId}", ref note, 500, new(-1, -1), true))
                        {
                            Logger.Information(note);
                            fProfile.Note = note.Trim();
                            this.config.LocalProfiles[fContentId] = fProfile;
                            this.config.Save();
                        }
                    }
                    // Display
                    else
                    {
                        SiGui.TextWrapped(string.IsNullOrWhiteSpace(note) ? "No note added for this player" : note);
                    }
                }
                ImGui.EndChild();

                // Editing toggle.
                if (ImGui.Button(this.editing ? "Save" : "Edit"))
                {
                    this.editing ^= true;
                }
            }
        }

        /// <summary>
        ///     Configuration for the user profile module.
        /// </summary>
        private sealed class UserProfilesModuleConfig : ModuleConfigBase
        {
            /// <inheritdoc />
            public override uint Version { get; protected set; }

            /// <inheritdoc />
            protected override string Identifier { get; set; } = "UserProfilesModule";

            public Dictionary<string, LocalProfile> LocalProfiles = new();
        }

        /// <summary>
        ///     Local user profile stores information that is only kept on the end users device.
        /// </summary>
        private record struct LocalProfile
        {
            /// <summary>
            ///     The note for the profile.
            /// </summary>
            public string Note { get; set; } = string.Empty;

            /// <summary>
            ///     Create a new profile.
            /// </summary>
            public LocalProfile()
            {

            }
        }
    }
}