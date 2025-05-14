using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using GoodFriend.Client.Http.Requests;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules;

internal sealed class InstanceInfoModule : BaseModule
{
    /// <summary>
    ///     The interval to update metadata at.
    /// </summary>
    private static readonly TimeSpan MetadataRefreshInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     The last time a metadata update was attempted.
    /// </summary>
    private DateTime lastMetadataUpdateAttempt = DateTime.MinValue;

    /// <summary>
    ///     The cached metadata value.
    /// </summary>
    private MetadataResponse? cachedMetadata;

    /// <summary>
    ///     Whether the last metadata update failed.
    /// </summary>
    private bool lastMetadataUpdateFailed;

    /// <inheritdoc />
    public override string Name { get; } = Strings.Modules_InstanceInfoModule_Name;

    /// <inheritdoc />
    public override ModuleTag Tag { get; } = ModuleTag.Connectivity;

    /// <inheritdoc />
    public override uint DisplayWeight { get; } = 1;

    /// <inheritdoc />
    protected override void OnEnable() { }

    /// <inheritdoc />
    protected override void OnDisable() { }

    /// <inheritdoc />
    protected override void DrawModule()
    {
        if (DateTime.Now - this.lastMetadataUpdateAttempt > MetadataRefreshInterval)
        {
            Task.Run(this.UpdateMetadataSafely);
        }

        if (!this.cachedMetadata.HasValue)
        {
            SiGui.TextWrappedColoured(this.lastMetadataUpdateFailed ? Colours.Error : Colours.Informational, this.lastMetadataUpdateFailed ? Strings.Modules_InstanceInfoModule_MetadataFetch_Failed : Strings.Modules_InstanceInfoModule_MetadataFetch_Fetching);
            return;
        }

        var metadata = this.cachedMetadata.Value;

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
        ImGui.Button(string.Format(Strings.Modules_InstanceInfoModule_ConnectedClients_Count, Math.Max(metadata.Connections.PlayerEvents, metadata.Connections.Announcements)), new(ImGui.GetContentRegionAvail().X, 0));
        ImGui.PopStyleColor(3);
        ImGui.Dummy(Spacing.SectionSpacing);

        if (ImGui.BeginChild("InstanceInfoChild"))
        {
            // Description.
            SiGui.Heading(Strings.Modules_InstanceInfoModule_Description_Title);
            if (string.IsNullOrWhiteSpace(metadata.About.Description))
            {
                SiGui.TextDisabledWrapped(Strings.Modules_InstanceInfoModule_Description_Unset);
            }
            else
            {
                SiGui.TextWrapped(metadata.About.Description);
            }
            ImGui.Dummy(Spacing.SectionSpacing);

            // Links.
            SiGui.Heading(Strings.Modules_InstanceInfoModule_CustomLinks_Title);
            if (metadata.About.CustomUrls.Count == 0)
            {
                SiGui.TextDisabledWrapped(Strings.Modules_InstanceInfoModule_CustomLinks_Unset);
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
            SiGui.TextWrappedColoured(Colours.Error, Strings.Modules_InstanceInfoModule_MetadataFetch_LastFailed);
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
            Logger.Debug("Updating metadata...");
            this.lastMetadataUpdateAttempt = DateTime.Now;
            var request = new GetMetadataRequest().Send(HttpClient, new());
            this.cachedMetadata = request.Item1;
            this.lastMetadataUpdateFailed = false;
            Logger.Debug($"Successfully updated metadata: {this.cachedMetadata.Value}");
        }
        catch (Exception e)
        {
            this.lastMetadataUpdateFailed = true;
            Logger.Warning($"Failed to get metadata: {e.Message}");
        }
    }
}
