using System.Collections.Generic;
using GoodFriend.Client.Responses;
using GoodFriend.Client.Responses.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    internal sealed class AnnouncementModule : ModuleBase
    {
        /// <inheritdoc />
        public override string Name => "Instance Announcements";

        /// <inheritdoc />
        public override string? Description => "Control what announcement you receive from the instance you are connected to.";

        /// <inheritdoc />
        public override ModuleTag Tag => ModuleTag.Notifications;

        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private AnnouncementModuleConfig Config { get; set; } = ModuleConfigBase.Load<AnnouncementModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction() => ApiClient.AnnouncementStream.OnStreamMessage += this.OnAnnouncementStreamMessage;

        /// <inheritdoc />
        protected override void DisableAction() => ApiClient.AnnouncementStream.OnStreamMessage -= this.OnAnnouncementStreamMessage;

        /// <inheritdoc />
        protected override void DrawModule()
        {
            SiGui.Heading("Announcement Display Options");
            foreach (var (kind, enabled) in this.Config.EnabledAnnouncements)
            {
                var isEnabled = enabled;
                if (SiGui.Checkbox($"Show '{kind}' announcements", ref isEnabled))
                {
                    if (!this.Config.EnabledAnnouncements.ContainsKey(kind))
                    {
                        this.Config.EnabledAnnouncements.Add(kind, isEnabled);
                    }
                    else
                    {
                        this.Config.EnabledAnnouncements[kind] = isEnabled;
                    }
                }
            }
        }

        /// <summary>
        ///     Called when an announcement is received.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rawEvent"></param>
        private unsafe void OnAnnouncementStreamMessage(object? _, AnnouncementStreamUpdate announcement)
        {
            if (string.IsNullOrWhiteSpace(announcement.Message))
            {
                return;
            }

            var shouldShow = this.Config.EnabledAnnouncements[announcement.Kind];
            if (!shouldShow)
            {
                return;
            }

            switch (announcement.Kind)
            {
                case AnnouncementKind.Critical:
                    ChatHelper.PrintImportant($"Announcement: {announcement.Message}");
                    break;
                default:
                    ChatHelper.Print($"Announcement: {announcement.Message}");
                    break;
            }
        }

        /// <summary>
        ///     Configuration for the world change module.
        /// </summary>
        internal sealed class AnnouncementModuleConfig : ModuleConfigBase
        {
            /// <inheritdoc />
            public override uint Version { get; protected set; }

            /// <inheritdoc />
            protected override string Identifier { get; set; } = "AnnouncementModule";

            /// <summary>
            ///    The announcements that should be shown.
            /// </summary>
            public Dictionary<AnnouncementKind, bool> EnabledAnnouncements { get; set; } = new()
            {
                { AnnouncementKind.Critical, true },
                { AnnouncementKind.Informational, true },
                { AnnouncementKind.Maintenance, true },
            };
        }
    }
}
