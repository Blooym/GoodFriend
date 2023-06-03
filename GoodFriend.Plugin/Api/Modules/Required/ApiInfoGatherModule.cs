using System;
using System.Linq;
using System.Timers;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.Api.Modules.Required
{
    internal sealed class InstanceInfoModule : ApiRequiredModule, IDisposable
    {
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

        /// <inheritdoc />
        public override string Name { get; } = "Instance Information";

        /// <inheritdoc />
        public override ApiModuleTag Tag { get; } = ApiModuleTag.Information;

        /// <inheritdoc />
        protected override void EnableAction()
        {
            this.updateMetadataTimer = new Timer(TimeSpan.FromMinutes(1));
            this.updateMetadataTimer.Elapsed += this.UpdateMetadataTimerOnElapsed;
            this.updateMetadataTimer.Start();

            this.UpdateMetadataSafely();
        }

        /// <inheritdoc />
        protected override void DisableAction()
        {
            this.updateMetadataTimer!.Stop();
            this.updateMetadataTimer!.Dispose();
            this.updateMetadataTimer = null;
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
            if (metadata.About.BannerUrl is not null)
            {
                SiGui.Image(metadata.About.BannerUrl.ToString(), ScalingMode.None, new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().X / 3));
            }
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudGrey3);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudGrey3);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.DalamudGrey3);
            ImGui.Button(metadata.About.Identifier, new(ImGui.GetContentRegionAvail().X, 0));
            ImGui.PopStyleColor(3);
            ImGui.Dummy(Spacing.SectionSpacing);

            if (this.lastMetadataUpdateFailed)
            {
                ImGui.Dummy(Spacing.SectionSpacing);
                SiGui.TextWrappedColoured(Colours.Error, "The last metadata update failed, currently using cached data.");
            }

            SiGui.Heading("Information");
            SiGui.TextWrapped($"There are currently {metadata.ConnectedClients} people connected to the API.");
            SiGui.TextWrapped($"The API has been running since {DateTimeOffset.FromUnixTimeSeconds(metadata.StartTime).ToLocalTime():F}.");
            ImGui.Dummy(Spacing.SectionSpacing);

            SiGui.Heading("Message of the Day");
            if (!metadata.About.Motd.Ignore)
            {
                switch (metadata.About.Motd.Important)
                {
                    case true:
                        SiGui.TextWrappedColoured(Colours.Important, metadata.About.Motd.Message);
                        break;
                    case false:
                        SiGui.TextWrapped(metadata.About.Motd.Message);
                        break;
                }
            }
            else
            {
                SiGui.TextDisabledWrapped("No message of the day has been set.");
            }
            ImGui.Dummy(Spacing.SectionSpacing);

            SiGui.Heading("API Links");
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
                Logger.Warning($"Failed to get metadata: {e}");
            }
        }

        /// <summary>
        ///     Called when the update metadata timer elapses to update the saved metadata.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateMetadataTimerOnElapsed(object? sender, ElapsedEventArgs e) => this.UpdateMetadataSafely();
    }
}