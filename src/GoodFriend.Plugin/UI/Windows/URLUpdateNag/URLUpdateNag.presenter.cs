namespace GoodFriend.UI.Windows.URLUpdateNag
{
    using System;
    using System.Linq;
    using GoodFriend.Base;
    using GoodFriend.Managers;
    using GoodFriend.Types;
    using Dalamud.Logging;
    using Dalamud.Game.ClientState.Conditions;

    public sealed class URLUpdateNagPresenter : IDisposable
    {
        /// <summary>
        ///     Dispose of the presenter.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        ///     Whether or not the nag has been dismissed this session.
        /// </summary>
        public bool URLUpdateNagDismissed { get; set; } = false;

        /// <summary>
        ///     Whether or not the nag needs to be shown.
        /// </summary>
        public bool ShowURLUpdateNag { get; set; } = false;

        /// <summary>
        ///     The URL to the new API instance.
        /// </summary>
        public Uri? NewAPIURL { get; private set; } = null;

        /// <summary>
        ///     Ignored new URLs that have been filtered for being invalid or insecure.
        /// </summary>
        public string[] IgnoredNewURLs { get; private set; } = new string[0];

        /// <summary>
        ///     Where to get metadata from.
        /// </summary>
        public APIClient.MetadataPayload? Metadata => PluginService.APIClientManager.GetMetadata();

        /// <summary>
        ///    Sets URLUpdateNag to true if the metadata has a set newApiUrl.
        /// </summary>
        public void HandleURLUpdateNag()
        {
            try
            {
                var newApiUrl = this.Metadata?.newApiUrl ?? null;
                if (newApiUrl != null && IgnoredNewURLs.All(u => u != newApiUrl) && newApiUrl != PluginService.Configuration.APIUrl.ToString())
                {
                    try
                    {
                        NewAPIURL = new Uri(newApiUrl);
                        if (NewAPIURL.Scheme == "https")
                        {
                            ShowURLUpdateNag = true;
                            PluginLog.Information($"URLUpdateNagPresenter(HandleURLUpdateNag): API recommended a new URL ({PluginService.Configuration.APIUrl} -> {NewAPIURL}) - showing user a nag if the haven't already dismissed it.");
                        }
                        else
                        {
                            IgnoredNewURLs = IgnoredNewURLs.Append(newApiUrl).ToArray();
                            PluginLog.Warning($"URLUpdateNagPresenter(HandleURLUpdateNag): API instance recommended changing to {newApiUrl} but it was not secure, ignoring.");
                            PluginService.EventLogManager.AddEntry($"Ignored insecure URL for new API instance ({newApiUrl}).", EventLogManager.EventLogType.Warning);
                        }
                    }
                    catch (UriFormatException)
                    {
                        if (NewAPIURL != null)
                        {
                            IgnoredNewURLs = IgnoredNewURLs.Append(newApiUrl).ToArray();
                            PluginLog.Warning($"URLUpdateNagPresenter(HandleURLUpdateNag): API instance recommended changing to {newApiUrl} but it was invalid, ignoring.");
                            PluginService.EventLogManager.AddEntry($"Ignored invalid URL for new API instance ({newApiUrl}).", EventLogManager.EventLogType.Warning);
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        ///     Checks to prevent showing the nag at inappropriate times.
        /// </summary>
        public bool CannotShowNag =>
            PluginService.Condition[ConditionFlag.InCombat]
            || PluginService.Condition[ConditionFlag.BoundByDuty]
            || PluginService.Condition[ConditionFlag.WatchingCutscene]
            || PluginService.ClientState.IsLoggedIn == false;
    }
}