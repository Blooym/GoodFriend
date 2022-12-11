using System;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.Managers;
using GoodFriend.Types;

namespace GoodFriend.UI.Windows.URLUpdateNag
{
    public sealed class URLUpdateNagPresenter : IDisposable
    {
        /// <summary>
        ///     Dispose of the presenter.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        ///    The configuration instance for the plugin.
        /// </summary>
        public static Configuration Configuration => PluginService.Configuration;

        /// <summary>
        ///     Whether or not the nag has been dismissed this session.
        /// </summary>
        public bool URLUpdateNagDismissed { get; set; }

        /// <summary>
        ///     Whether or not the nag needs to be shown.
        /// </summary>
        public bool ShowURLUpdateNag { get; set; }

        /// <summary>
        ///     The URL to the new API instance.
        /// </summary>
        public Uri? NewAPIURL { get; private set; }

        /// <summary>
        ///     Ignored new URLs that have been filtered for being invalid or insecure.
        /// </summary>
        public string[] IgnoredNewURLs { get; private set; } = Array.Empty<string>();

        /// <summary>
        ///     Where to get metadata from.
        /// </summary>
        public static APIClient.MetadataPayload? Metadata => PluginService.APIClientManager?.MetadataCache;

        /// <summary>
        ///    Sets URLUpdateNag to true if the metadata has a set newApiUrl.
        /// </summary>
        public void HandleURLUpdateNag()
        {
            var newApiUrl = Metadata?.NewApiUrl;

            // Check if the string is null or empty or equal to the current API URL.
            if (string.IsNullOrEmpty(newApiUrl) || newApiUrl == Configuration.APIUrl.ToString())
            { return; }

            // If the URL has been ignored this session, don't show the nag.
            if (this.IgnoredNewURLs.Contains(newApiUrl))
            { return; }

            try
            {
                // Try and format the URL.
                this.NewAPIURL = new Uri(newApiUrl);

                // If the format is HTTPs, show the nag.
                if (this.NewAPIURL.Scheme == "https")
                {
                    this.ShowURLUpdateNag = true;
                    PluginLog.Information($"URLUpdateNagPresenter(HandleURLUpdateNag): API recommended a new URL ({Configuration.APIUrl} -> {this.NewAPIURL}) - showing user a nag if the haven't already dismissed it.");
                    PluginService.EventLogManager.AddEntry($"API recommended moving to a new URL ({Configuration.APIUrl} -> {this.NewAPIURL}).", EventLogManager.EventLogType.Info);
                }

                // Otherwise, ignore the URL and move on.
                else
                {
                    this.IgnoredNewURLs = this.IgnoredNewURLs.Append(newApiUrl).ToArray();
                    PluginLog.Warning($"URLUpdateNagPresenter(HandleURLUpdateNag): API instance recommended changing to {newApiUrl} but it was not secure, ignoring.");
                    PluginService.EventLogManager.AddEntry($"Ignored insecure URL for new API instance ({newApiUrl}).", EventLogManager.EventLogType.Warning);
                }
            }
            catch (UriFormatException)
            {
                // If the URL is invalid, ignore it and move on.
                if (this.NewAPIURL != null)
                {
                    this.IgnoredNewURLs = this.IgnoredNewURLs.Append(newApiUrl).ToArray();
                    PluginLog.Warning($"URLUpdateNagPresenter(HandleURLUpdateNag): API instance recommended changing to {newApiUrl} but it was invalid, ignoring.");
                    PluginService.EventLogManager.AddEntry($"Ignored invalid URL for new API instance ({newApiUrl}).", EventLogManager.EventLogType.Warning);
                }
            }
        }

        /// <summary>
        ///     Checks to prevent showing the nag at inappropriate times.
        /// </summary>
        public static bool CannotShowNag =>
            PluginService.Condition[ConditionFlag.InCombat]
            || PluginService.Condition[ConditionFlag.BoundByDuty]
            || PluginService.Condition[ConditionFlag.WatchingCutscene]
            || !PluginService.ClientState.IsLoggedIn;
    }
}
