using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.Enums;
using GoodFriend.Localization;
using GoodFriend.Managers.FriendList;
using GoodFriend.Types;
using GoodFriend.Utils;
using Newtonsoft.Json;
using ToastType = Dalamud.Interface.Internal.Notifications.NotificationType;

namespace GoodFriend.Managers
{
    /// <summary>
    ///     Manages an APIClient and its resources and handles events.
    /// </summary>
    public sealed class APIClientManager : IDisposable
    {
        /// <summary>
        ///    The current player ContentID, saved on login and cleared on logout.
        /// </summary>
        private ulong currentContentId;

        /// <summary>
        ///    The current player HomeworldID. Saved on login and cleared on logout.
        /// </summary>
        private uint currentHomeworldId;

        /// <summary>
        ///    The current player WorldID. Saved on login/world change and cleared on logout.
        /// </summary>
        private uint currentWorldId;

        /// <summary>
        ///    The current player DataCenterID. Saved on login/world change and cleared on logout.
        /// </summary>
        private uint currentDatacenterId;

        /// <summary>
        ///    The current player TerritoryID. Saved on login/zone change and cleared on logout.
        /// </summary>
        private uint currentTerritoryId;

        /// <summary>
        ///    The cached metadata from the API.
        /// </summary>
        public APIClient.MetadataPayload? MetadataCache { get; private set; }

        /// <summary>
        ///    The timer for updating the metadata cache.
        /// </summary>
        private readonly Timer metadataTimer = new(240000);

        /// <summary>
        ///    The event that fires when the metadata timer elapses.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMetadataTimerElapsed(object? source, ElapsedEventArgs e)
        {
            PluginLog.Verbose("APIClientManager(OnMetadataRefresh): Metadata timer elapsed, updating metadata cache.");
            this.UpdateMetadataCache();
        }

        /// <summary>
        ///     The API Client associated with the manager.
        /// </summary>
        private readonly APIClient apiClient = new(PluginService.Configuration);

        /// <summary>
        ///     The APIClient version string.
        /// </summary>
        public static string Version => APIClient.ApiVersion;

        /// <summary>
        ///     The ClientState associated with the manager
        /// </summary>
        private readonly ClientState clientState;

        /// <summary>
        ///     The Framework associated with the manager
        /// </summary>
        private readonly Framework framework;

        /// <summary>
        ///     Instantiates a new APIClientManager.
        /// </summary>
        /// <param name="clientState">The ClientState to use.</param>
        /// <param name="framework">The Framework to use.</param>
        public APIClientManager(ClientState clientState, Framework framework)
        {
            PluginLog.Debug("APIClientManager(APIClientManager): Initializing...");

            this.clientState = clientState;
            this.framework = framework;
            this.UpdateMetadataCache();

            // Create event handlers
            this.apiClient.SSEDataReceived += this.OnSSEDataReceived;
            this.apiClient.SSEConnectionError += this.OnSSEAPIClientError;
            this.apiClient.SSEConnectionEstablished += this.OnSSEAPIClientConnected;
            this.apiClient.SSEConnectionClosed += this.OnSSEAPIClientDisconnected;
            this.apiClient.RequestError += this.OnRequestError;
            this.apiClient.RequestSuccess += this.OnRequestSuccess;
            this.clientState.Login += this.OnLogin;
            this.clientState.Logout += this.OnLogout;
            this.framework.Update += this.OnFrameworkUpdate;

            // Metadata cache update timer
            this.metadataTimer.Elapsed += this.OnMetadataTimerElapsed;
            this.metadataTimer.AutoReset = true;
            this.metadataTimer.Enabled = true;

            // If the player is logged in already, connect & set their ID.
            if (this.clientState.LocalPlayer != null)
            {
                this.currentContentId = this.clientState.LocalContentId;
                this.currentHomeworldId = this.clientState.LocalPlayer.HomeWorld.Id;
                this.currentTerritoryId = this.clientState.TerritoryType;
                this.currentWorldId = this.clientState.LocalPlayer.CurrentWorld.Id;
                this.currentDatacenterId = this.clientState.LocalPlayer.CurrentWorld.GameData?.DataCenter.Row ?? 0;
                PluginLog.Information("APIClientManager(APIClientManager): Player is already logged in, setting IDs and connecting to API.");
                this.apiClient.OpenSSEStream();
            }

            PluginLog.Debug("APIClientManager(APIClientManager): Successfully initialized.");
        }

        /// <summary>
        ///     Disposes of the APIClientManager and its resources.
        /// </summary>
        public void Dispose()
        {
            this.apiClient.Dispose();
            this.clientState.Login -= this.OnLogin;
            this.clientState.Logout -= this.OnLogout;
            this.framework.Update -= this.OnFrameworkUpdate;
            this.apiClient.SSEDataReceived -= this.OnSSEDataReceived;
            this.apiClient.SSEConnectionError -= this.OnSSEAPIClientError;
            this.apiClient.SSEConnectionEstablished -= this.OnSSEAPIClientConnected;
            this.apiClient.SSEConnectionClosed -= this.OnSSEAPIClientDisconnected;
            this.apiClient.RequestError -= this.OnRequestError;
            this.apiClient.RequestSuccess -= this.OnRequestSuccess;

            this.metadataTimer.Elapsed -= this.OnMetadataTimerElapsed;
            this.metadataTimer.Stop();
            this.metadataTimer.Dispose();

            PluginLog.Debug("APIClientManager(Dispose): Successfully disposed.");
        }

        /// <summary>
        ///     Handles the login event by opening the APIClient stream and sending a login.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private unsafe void OnLogin(object? sender, EventArgs e)
        {
            if (!this.apiClient.SSEIsConnected)
            {
                this.apiClient.OpenSSEStream();
            }

            Task.Run(() =>
          {
              while (this.clientState.LocalContentId == 0 || this.clientState.LocalPlayer == null)
              {
                  Task.Delay(100).Wait();
              }

              // Set the current IDs
              this.currentContentId = this.clientState.LocalContentId;
              this.currentHomeworldId = this.clientState.LocalPlayer.HomeWorld.Id;
              this.currentTerritoryId = this.clientState.TerritoryType;
              this.currentWorldId = this.clientState.LocalPlayer.CurrentWorld.Id;
              this.currentDatacenterId = this.clientState.LocalPlayer.CurrentWorld?.GameData?.DataCenter.Row ?? 0;

              // Send a login
              this.apiClient.SendLogin(this.currentContentId, this.currentHomeworldId, this.currentWorldId, this.currentTerritoryId, this.currentDatacenterId);
              PluginLog.Information("APIClientManager(OnLogin): successfully set stored IDs for the APIClientManager.");
          });
        }

        /// <summary>
        ///     Handles the logout event by closing the APIClient stream and sending a logout.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (this.apiClient.SSEIsConnected)
            {
                this.apiClient.CloseSSEStream();
            }

            // Cancel any pending requests and send a logout.
            this.apiClient.CancelPendingRequests();
            this.apiClient.SendLogout(this.currentContentId, this.currentHomeworldId, this.currentWorldId, this.currentTerritoryId, this.currentDatacenterId);

            // Clear the current IDs
            this.currentTerritoryId = 0;
            this.currentWorldId = 0;
            this.currentHomeworldId = 0;
            this.currentContentId = 0;
            this.currentDatacenterId = 0;
            this.hasConnectedSinceError = true;

            PluginLog.Information("APIClientManager(OnLogout): successfully cleared stored IDs for the APIClientManager.");
        }

        /// <summary>
        ///     Handles the framework update event.
        /// </summary>
        /// <param name="framework"></param>
        private void OnFrameworkUpdate(Framework framework)
        {
            var currentWorld = this.clientState.LocalPlayer?.CurrentWorld;
            var currentTerritory = this.clientState.TerritoryType;

            if (currentWorld != null && currentWorld.Id != 0 && currentWorld.Id != this.currentWorldId)
            {
                PluginLog.Debug($"APIClientManager(OnFrameworkUpdate): Stored WorldID changed from {this.currentWorldId} to {currentWorld.Id}.");
                this.currentWorldId = currentWorld.Id;
            }

            if (currentTerritory != 0 && currentTerritory != this.currentTerritoryId)
            {
                PluginLog.Debug($"APIClientManager(OnFrameworkUpdate): Stored TerritoryID changed from {this.currentTerritoryId} to {currentTerritory}.");
                this.currentTerritoryId = currentTerritory;
            }
        }

        /// <summary>
        ///     Handles data received from the APIClient.
        /// </summary>
        /// <param name="data">The data received.</param>
        private unsafe void OnSSEDataReceived(APIClient.UpdatePayload data)
        {
            // Don't process the data if the ContentID is null.
            if (data.ContentID == null)
            {
                PluginLog.Verbose("APIClientManager(OnSSEDataReceived): Event was malformed due to a null ContentID, ignoring.");
                return;
            }

            // Don't process the data if the hash matches the current player's hash.
            if (data.ContentID == CryptoUtil.HashSHA512(this.currentContentId.ToString(), data.Salt ?? string.Empty))
            {
                PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): ContentID is the same as the current character's ContentID, skipping.");
                return;
            }

            PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Data received from API, processing it ({data.ContentID[..8]}...).");

            // Find if the friend is on the users friend list.
            FriendListEntry* friend = default;
            foreach (FriendListEntry* x in FriendList.FriendList.Get())
            {
#if DEBUG
                PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Checking {x->Name} for match.");
#endif
                var hash = CryptoUtil.HashSHA512(x->ContentId.ToString(), data.Salt ?? string.Empty);
                if (hash == data.ContentID)
                {
                    friend = x;
                    break;
                }
            }

            // If friend is null, the user is not added by the client or the ID is invalid.
            if (friend == null || data == null)
            {
                PluginLog.Verbose("APIClientManager(OnSSEDataReceived): No friend found from the event, cannot display to client.");
                return;
            }

            // If the player has "Hide Same FC" enabled, check if the FC matches the current player's FC.
            if (this.clientState.LocalPlayer?.CompanyTag?.ToString() != string.Empty
                && friend->FreeCompany.ToString() == this.clientState?.LocalPlayer?.CompanyTag.ToString()
                && friend->HomeWorld == this.clientState.LocalPlayer.HomeWorld.Id
                && PluginService.Configuration.HideSameFC)
            {
                PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to sharing the same free company & homeworld. (FC: {friend->FreeCompany})");
                return;
            }

            // If the player has "Hide Different Homeworld" enabled, check if the home world matches the current player's home world.
            if (this.currentHomeworldId != friend->HomeWorld && PluginService.Configuration.HideDifferentHomeworld)
            {
                PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being on a different homeworld. (Them: {friend->HomeWorld}, You: {this.currentHomeworldId})");
                return;
            }

            //// If the player has "Hide Different World" enabled, check if the world matches the current player's world.
            if (this.currentWorldId != data.WorldID && PluginService.Configuration.HideDifferentWorld && data.WorldID != 0)
            {
                PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being on a different world. (Them: {data.WorldID}, You: {this.currentWorldId})");
                return;
            }

            // If the player has "Hide Different Datacenter" enabled, check if the datacenter matches the current player's datacenter.
            if (this.currentDatacenterId != data.DatacenterID && PluginService.Configuration.HideDifferentDatacenter)
            {
                PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being on a different datacenter. (Them: {data.DatacenterID}, You: {this.currentDatacenterId})");
                return;
            }

            // If the player has "Hide Different Territory" enabled, check if the territory matches the current player's territory.
            if (data.TerritoryID != this.currentTerritoryId && PluginService.Configuration.HideDifferentTerritory)
            {
                PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being for a different territory. (Them: {data.TerritoryID}, You: {this.currentTerritoryId})");
                return;
            }

            // Everything is good, send the event to the player and add it to the log.
            PluginLog.Debug($"APIClientManager(OnSSEDataReceived): {friend->Name} {(data.LoggedIn ? Events.LoggedIn : Events.LoggedOut)}");
            PluginService.EventLogManager.AddEntry($"{friend->Name} {(data.LoggedIn ? Events.LoggedIn : Events.LoggedOut)}", EventLogManager.EventLogType.Info);
            Notifications.ShowPreferred(data.LoggedIn ?
            string.Format(PluginService.Configuration.FriendLoggedInMessage, friend->Name, friend->FreeCompany)
            : string.Format(PluginService.Configuration.FriendLoggedOutMessage, friend->Name, friend->FreeCompany), ToastType.Info);
        }

        /// <summary>
        ///     Stores if the API has connected since the last time an error was shown.
        /// </summary>
        private bool hasConnectedSinceError = true;

        /// <summary>
        ///     Handles the APIClient error event.
        /// </summary>
        /// <param name="e">The error.</param>
        private void OnSSEAPIClientError(Exception e)
        {
            this.hasConnectedSinceError = false;
            PluginLog.Error($"APIClientManager(OnSSEAPIClientError): A connection error occured: {e.Message}");
            PluginService.EventLogManager.AddEntry($"{Events.APIConnectionError}: {e.Message}", EventLogManager.EventLogType.Error);
            if (this.hasConnectedSinceError && PluginService.Configuration.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionError, NotificationType.Toast, ToastType.Error);
            }
        }

        /// <summary>
        ///     Handles the APIClient connected event.
        /// </summary>
        private void OnSSEAPIClientConnected()
        {
            this.hasConnectedSinceError = true;
            PluginLog.Information("APIClientManager(OnSSEAPIClientConnected): Successfully connected to the API.");
            PluginService.EventLogManager.AddEntry(Events.APIConnectionSuccess, EventLogManager.EventLogType.Info);

            if (PluginService.Configuration.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionSuccess, NotificationType.Toast, ToastType.Success);
            }
        }

        /// <summary>
        ///     Handles the APIClient disconnected event.
        /// </summary>
        private void OnSSEAPIClientDisconnected()
        {
            this.hasConnectedSinceError = false;
            PluginLog.Information("APIClientManager(OnSSEAPIClientDisconnected): Disconnected from the API.");
            PluginService.EventLogManager.AddEntry(Events.APIConnectionDisconnected, EventLogManager.EventLogType.Info);
        }

        /// <summary>
        ///     Handles the APIClient RequestError event.
        /// </summary>
        /// <param name="e">The error.</param>
        /// <param name="response">The request.</param>
        private void OnRequestError(Exception e, HttpResponseMessage? response)
        {
            var uri = response?.RequestMessage?.RequestUri?.ToString().Split('?')[0];

            if (response?.StatusCode == HttpStatusCode.TooManyRequests)
            {
                PluginService.EventLogManager.AddEntry($"{Events.APIConnectionRatelimited}", EventLogManager.EventLogType.Warning);
            }
            else
            {
                PluginLog.Error($"APIClientManager(OnRequestError): An error occurred while making a request to the API: {e.Message}");
                PluginService.EventLogManager.AddEntry($"{e.Message} ({uri})", EventLogManager.EventLogType.Error);
            }
        }

        /// <summary>
        ///     Handles the APIClient RequestSuccess event.
        /// </summary>
        /// <param name="response">The request.</param>
        private void OnRequestSuccess(HttpResponseMessage response)
        {
            var uri = response.RequestMessage?.RequestUri?.ToString().Split('?')[0];
            PluginLog.Information($"APIClientManager(OnRequestSuccess): Request to {uri} was successful with status code {response.StatusCode}.");
            PluginService.EventLogManager.AddEntry($"{response.StatusCode} ({uri})", EventLogManager.EventLogType.Debug);
        }

        /// <summary>
        ///     Get the connection status of the API.
        /// </summary>
        public ConnectionStatus GetConnectionStatus()
        {
            if (this.apiClient.LastStatusCode == HttpStatusCode.TooManyRequests)
            {
                return ConnectionStatus.Ratelimited;
            }
            else if (!this.hasConnectedSinceError)
            {
                return ConnectionStatus.Error;
            }
            else if (this.apiClient.SSEIsConnected)
            {
                return ConnectionStatus.Connected;
            }
            else if (this.apiClient.SSEIsConnecting)
            {
                return ConnectionStatus.Connecting;
            }
            else
            {
                return ConnectionStatus.Disconnected;
            }
        }

        private void UpdateMetadataCache()
        {
            var metadata = this.apiClient.GetMetadata();
            if (metadata != null)
            {
                this.MetadataCache = metadata;
                PluginLog.Debug($"APIClientManager(GetMetadata): Metadata cache updated {JsonConvert.SerializeObject(this.MetadataCache)}");
            }
            else
            {
                PluginLog.Debug("APIClientManager(GetMetadata): Unable to update metadata cache.");
            }
        }
    }
}
