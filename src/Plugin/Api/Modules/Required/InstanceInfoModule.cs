using System;
using System.Linq;
using System.Timers;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.Api.Modules.Required
{
    internal sealed class InstanceInfoModule : ApiRequiredModule, IDisposable
    {
        /// <summary>
        ///     The command ID for the MOTD unsubscribe command.
        /// </summary>
        private const uint MotdCommandId = 1001;

        /// <summary>
        ///     The interval at which to update the metadata.
        /// </summary>
        private static readonly TimeSpan MetadataUpdateInterval = TimeSpan.FromMinutes(2);

        /// <summary>
        ///     The timer used to update the metadata.
        /// </summary>
        private Timer? updateMetadataTimer;

        /// <summary>
        ///     The last time the metadata was updated.
        /// </summary>
        private MetadataResponse? metadata;

        /// <summary>
        ///     Whether or not the last metadata update failed.
        /// </summary>
        private bool lastMetadataUpdateFailed;

        /// <summary>
        ///     The payload used to unsubscribe from the MOTD link.
        /// </summary>
        private DalamudLinkPayload? unsubMotdLinkPayload;

        /// <inheritdoc />
        public override string Name { get; } = "About Instance";

        /// <inheritdoc />
        public override ApiModuleTag Tag { get; } = ApiModuleTag.Information;


        /// <inheritdoc />
        public override uint DisplayWeight { get; } = 1;

        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        private InstanceInfoModuleConfig Config { get; set; } = ApiModuleConfigBase.Load<InstanceInfoModuleConfig>();

        /// <inheritdoc />
        protected override void EnableAction()
        {
            this.updateMetadataTimer = new Timer(MetadataUpdateInterval);
            this.updateMetadataTimer.Elapsed += this.UpdateMetadataTimerOnElapsed;
            this.updateMetadataTimer.Start();

            DalamudInjections.ClientState.Login += this.OnLogin;

            this.UpdateMetadataSafely();

            this.unsubMotdLinkPayload = DalamudInjections.PluginInterface.AddChatLinkHandler(MotdCommandId, (i, m) =>
            {
                if (!this.Config.ShowMotdOnLogin)
                {
                    ChatHelper.PrintError("You have already disabled messages of the day.");
                    return;
                }

                this.Config.ShowMotdOnLogin = false;
                this.Config.Save();
                ChatHelper.Print("You will no longer see messages of the day when you log in. You can re-enable them in the Instance Info settings.");
            });
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            this.updateMetadataTimer!.Stop();
            this.updateMetadataTimer!.Dispose();
            this.updateMetadataTimer = null;

            DalamudInjections.ClientState.Login -= this.OnLogin;
            DalamudInjections.PluginInterface.RemoveChatLinkHandler(MotdCommandId);
        }

        /// <inheritdoc />
        public void Dispose() => this.Disable();

        /// <inheritdoc />
        protected override void DrawModule()
        {
            if (!this.metadata.HasValue)
            {
                SiGui.TextWrappedColoured(this.lastMetadataUpdateFailed ? Colours.Error : Colours.Informational, this.lastMetadataUpdateFailed ? "Failed to fetch metadata, will try again later." : "Fetching metadata...");
                return;
            }

            var metadata = this.metadata.Value;
            if (!string.IsNullOrEmpty(metadata.About.BannerUrl.ToString()))
            {
                SiGui.Image(metadata.About.BannerUrl.ToString(), ScalingMode.None, new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().X / 3));
            }

            // Server name
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudGrey3);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudGrey3);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.DalamudGrey3);
            ImGui.Button(metadata.About.Identifier, new(ImGui.GetContentRegionAvail().X, 40));
            ImGui.PopStyleColor(3);

            // Clients connected
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.ParsedBlue);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.ParsedBlue);
            ImGui.Button($"There are currently {metadata.ConnectedClients} players online", new(ImGui.GetContentRegionAvail().X, 0));
            ImGui.PopStyleColor(3);
            ImGui.Dummy(Spacing.SectionSpacing);

            if (ImGui.BeginChild("InstanceInfoChild"))
            {
                // MOTD.
                SiGui.Heading("Message of the Day");
                if (!metadata.About.Motd.Ignore)
                {
                    SiGui.TextWrapped(metadata.About.Motd.Message);
                }
                else
                {
                    SiGui.TextDisabledWrapped("No message of the day has been set.");
                }
                ImGui.Dummy(Spacing.ReadableSpacing);

                // Motd toggle button.
                var showOnLogin = this.Config.ShowMotdOnLogin;
                if (SiGui.Checkbox("Show MOTD on login", ref showOnLogin))
                {
                    this.Config.ShowMotdOnLogin = showOnLogin;
                    this.Config.Save();
                }
                ImGui.Dummy(Spacing.SectionSpacing);

                // Links.
                SiGui.Heading("Custom Links");
                if (metadata.About.CustomUrls.Count == 0)
                {
                    SiGui.TextDisabledWrapped("No custom links have been set.");
                }
                else
                {
                    foreach (var link in metadata.About.CustomUrls.OrderBy(x => x.Key))
                    {
                        var url = link.Value.ToString();
                        if (ImGui.Selectable(link.Key, false))
                        {
                            Util.OpenLink(url);
                        }
                        SiGui.AddTooltip(url);
                    }
                    ImGui.Dummy(Spacing.ReadableSpacing);
                }
            }

            if (this.lastMetadataUpdateFailed)
            {
                ImGui.Dummy(Spacing.SectionSpacing);
                SiGui.TextWrappedColoured(Colours.Error, "The last metadata update failed, currently using cached data.");
            }

            ImGui.EndChild();
        }

        /// <summary>
        ///     Updates the metadata safely.
        /// </summary>
        private void UpdateMetadataSafely()
        {
            try
            {
                var request = ApiClient.GetMetadata();
                this.metadata = request.Item1;
                this.lastMetadataUpdateFailed = false;
            }
            catch (Exception e)
            {
                this.lastMetadataUpdateFailed = true;
                Logger.Warning($"Failed to get metadata: {e.Message}");
            }
        }

        /// <summary>
        ///     Displays the message of the day when the player logs in if enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogin(object? sender, EventArgs? e)
        {
            if (!this.Config.ShowMotdOnLogin || !this.metadata.HasValue || this.metadata.Value.About.Motd.Ignore)
            {
                return;
            }

            Logger.Information($"Displaying message of the day: {this.metadata.Value.About.Motd.Message}");
            ChatHelper.Print(new SeStringBuilder()
                        .AddText(this.metadata.Value.About.Motd.Message)
                        .AddText(" ")
                        .Add(this.unsubMotdLinkPayload!)
                        .AddUiForeground((ushort)ChatUiColourKey.Orange)
                        .AddText("[Disable messages on login]")
                        .AddUiForegroundOff()
                        .Add(RawPayload.LinkTerminator)
                        .Build());
        }

        /// <summary>
        ///     Called when the update metadata timer elapses to update the saved metadata.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateMetadataTimerOnElapsed(object? sender, ElapsedEventArgs e) => this.UpdateMetadataSafely();
    }

    /// <inheritdoc />
    internal sealed class InstanceInfoModuleConfig : ApiRequiredModuleConfig
    {
        /// <inheritdoc />
        public override uint Version { get; protected set; }

        /// <inheritdoc />
        protected override string Identifier { get; set; } = "InstanceInfo";

        /// <summary>
        ///     Whether or not to show the MOTD on login.
        /// </summary>
        public bool ShowMotdOnLogin { get; set; } = true;
    }
}
