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
        private void OnMetadataTimerElapsed(object? source, ElapsedEventArgs e)
        {
            PluginLog.Verbose($"APIClientManager(OnMetadataRefresh): Metadata timer elapsed, updating metadata cache.");
            this.UpdateMetadataCache();
        }

        /// <summary>
        ///     The API Client associated with the manager.
        /// </summary>
        private APIClient APIClient { get; set; } = new APIClient(PluginService.Configuration);

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
        public APIClientManager(ClientState clientState, Framework framework)
        {
            PluginLog.Debug("APIClientManager(APIClientManager): Initializing...");

            this.clientState = clientState;
            this.framework = framework;
            this.UpdateMetadataCache();

            // Create event handlers
            this.APIClient.SSEDataReceived += this.OnSSEDataReceived;
            this.APIClient.SSEConnectionError += this.OnSSEAPIClientError;
            this.APIClient.SSEConnectionEstablished += this.OnSSEAPIClientConnected;
            this.APIClient.SSEConnectionClosed += this.OnSSEAPIClientDisconnected;
            this.APIClient.RequestError += this.OnRequestError;
            this.APIClient.RequestSuccess += this.OnRequestSuccess;
            this.clientState.Login += this.OnLogin;
            this.clientState.Logout += this.OnLogout;
            this.clientState.TerritoryChanged += this.OnTerritoryChange;
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
                this.APIClient.OpenSSEStream();
            }

            PluginLog.Debug("APIClientManager(APIClientManager): Successfully initialized.");
        }

        /// <summary>
        ///     Disposes of the APIClientManager and its resources.
        /// </summary>
        public void Dispose()
        {
            this.APIClient.Dispose();
            this.clientState.Login -= this.OnLogin;
            this.clientState.Logout -= this.OnLogout;
            this.clientState.TerritoryChanged -= this.OnTerritoryChange;
            this.framework.Update -= this.OnFrameworkUpdate;
            this.APIClient.SSEDataReceived -= this.OnSSEDataReceived;
            this.APIClient.SSEConnectionError -= this.OnSSEAPIClientError;
            this.APIClient.SSEConnectionEstablished -= this.OnSSEAPIClientConnected;
            this.APIClient.SSEConnectionClosed -= this.OnSSEAPIClientDisconnected;
            this.APIClient.RequestError -= this.OnRequestError;
            this.APIClient.RequestSuccess -= this.OnRequestSuccess;

            this.metadataTimer.Elapsed -= this.OnMetadataTimerElapsed;
            this.metadataTimer.Stop();
            this.metadataTimer.Dispose();

            PluginLog.Debug("APIClientManager(Dispose): Successfully disposed.");
        }

        /// <summary>
        ///     Handles the login event by opening the APIClient stream and sending a login.
        /// </summary>
        private unsafe void OnLogin(object? sender, EventArgs e)
        {
            if (!this.APIClient.SSEIsConnected)
            {
                this.APIClient.OpenSSEStream();
            }

            Task.Run(() =>
          {
              while (this.clientState.LocalContentId == 0 || this.clientState.LocalPlayer == null)
              {
                  Task.Delay(100).Wait();
              }

              this.currentContentId = this.clientState.LocalContentId;
              this.currentHomeworldId = this.clientState.LocalPlayer.HomeWorld.Id;
              this.currentTerritoryId = this.clientState.TerritoryType;
              this.currentWorldId = this.clientState.LocalPlayer.CurrentWorld.Id;
              this.currentDatacenterId = this.clientState.LocalPlayer.CurrentWorld?.GameData?.DataCenter.Row ?? 0;
              this.APIClient.SendLogin(this.currentContentId, this.currentHomeworldId, this.currentWorldId, this.currentTerritoryId, this.currentDatacenterId);
              PluginLog.Information($"APIClientManager(OnLogin): Stored IDs set to: Homeworld: {this.currentHomeworldId}, World: {this.currentWorldId}, Datacenter: {this.currentDatacenterId}, Content: [REDACTED], Territory: {this.currentTerritoryId}");
          });
        }

        /// <summary>
        ///     Handles the logout event by closing the APIClient stream and sending a logout.
        /// </summary>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (this.APIClient.SSEIsConnected)
            {
                this.APIClient.CloseSSEStream();
            }

            this.APIClient.CancelPendingRequests();
            this.APIClient.SendLogout(this.currentContentId, this.currentHomeworldId, this.currentWorldId, this.currentTerritoryId, this.currentDatacenterId);
            this.currentTerritoryId = 0;
            this.currentWorldId = 0;
            this.currentHomeworldId = 0;
            this.currentContentId = 0;
            this.currentDatacenterId = 0;
            this.hasConnectedSinceError = true;
            PluginLog.Information("APIClientManager(OnLogout): Player logged out, stored IDs reset to 0.");

        }

        /// <summary>
        ///     Handles the territory change event.
        /// </summary>
        private void OnTerritoryChange(object? sender, ushort newId)
        {
            PluginLog.Debug($"APIClientManager(OnTerritoryChange): Stored TerritoryID changed from {this.currentTerritoryId} to {newId}.");
            this.currentTerritoryId = newId;
        }

        /// <summary>
        ///     Handles the framework update event.
        /// </summary>
        private void OnFrameworkUpdate(Framework framework)
        {
            var currentWorld = this.clientState.LocalPlayer?.CurrentWorld;

            if (currentWorld != null && currentWorld.Id != 0 && currentWorld.Id != this.currentWorldId)
            {
                PluginLog.Debug($"APIClientManager(OnFrameworkUpdate): Stored WorldID changed from {this.currentWorldId} to {currentWorld.Id}.");
                this.currentWorldId = currentWorld.Id;
            }
        }

        /// <summary>
        ///     Handles data received from the APIClient.
        /// </summary>
        private unsafe void OnSSEDataReceived(APIClient.UpdatePayload data)
        {
            if (data.ContentID == Hashing.HashSHA512(this.currentContentId.ToString()))
            {
                return;
            }

            PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Data received from API, processing.");

            // Don't process the data if the ContentID is null.
            if (data.ContentID == null)
            {
                PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Event was malformed due to a null ContentID, ignoring.");
                return;
            }

            // Find if the friend is on the users friend list.
            FriendListEntry* friend = default;
            foreach (FriendListEntry* x in FriendList.FriendList.Get())
            {
                var hash = Hashing.HashSHA512(x->ContentId.ToString());
                if (hash == data.ContentID)
                {
                    friend = x;
                    break;
                }
            }

            // If friend is null, the user is not added by the client.
            if (friend == null || data == null)
            {
                PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): No friend found from the event, cannot display to client.");
                return;
            }

            // Client has a friend with the same ContentID hash as the event.
            if (this.clientState.LocalPlayer?.CompanyTag?.ToString() != string.Empty
                && friend->FreeCompany.ToString() == this.clientState?.LocalPlayer?.CompanyTag.ToString()
                && friend->HomeWorld == this.clientState.LocalPlayer.HomeWorld.Id
                && PluginService.Configuration.HideSameFC)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to sharing the same free company & homeworld. (FC: {friend->FreeCompany})");
                return;
            }

            if (this.currentHomeworldId != friend->HomeWorld && PluginService.Configuration.HideDifferentHomeworld)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different homeworld. (Them: {friend->HomeWorld}, You: {this.currentHomeworldId})");
                return;
            }

            if (this.currentWorldId != data.WorldID && PluginService.Configuration.HideDifferentWorld && data.WorldID != null && data.WorldID != 0)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different world. (Them: {data.WorldID}, You: {this.currentWorldId})");
                return;
            }

            if (this.currentDatacenterId != data.DatacenterID && PluginService.Configuration.HideDifferentDatacenter)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different datacenter. (Them: {data.DatacenterID}, You: {this.currentDatacenterId})");
                return;
            }

            // If the event is not for the current territory, ignore it.
            if (data.TerritoryID != this.currentTerritoryId && PluginService.Configuration.HideDifferentTerritory)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being for a different territory. (Them: {data.TerritoryID}, You: {this.currentTerritoryId})");
                return;
            }

            // Notify the client and add the event to the logs.
            PluginService.EventLogManager.AddEntry($"{friend->Name} {(data.LoggedIn ? Events.LoggedIn : Events.LoggedOut)}", EventLogManager.EventLogType.Info);
            PluginLog.Information($"APIClientManager(OnSSEDataReceived): {friend->Name} {(data.LoggedIn ? Events.LoggedIn : Events.LoggedOut)}");
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
            PluginLog.Information($"APIClientManager(OnSSEAPIClientDisconnected): Disconnected from the API.");
            PluginService.EventLogManager.AddEntry(Events.APIConnectionDisconnected, EventLogManager.EventLogType.Info);
        }

        /// <summary>
        ///     Handles the APIClient RequestError event.
        /// </summary>
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
        private void OnRequestSuccess(HttpResponseMessage response)
        {
            var uri = response.RequestMessage?.RequestUri?.ToString().Split('?')[0];
            PluginLog.Debug($"APIClientManager(OnRequestSuccess): Request to {uri} was successful with status code {response.StatusCode}.");
            PluginService.EventLogManager.AddEntry($"{response.StatusCode} ({uri})", EventLogManager.EventLogType.Debug);
        }

        /// <summary>
        ///     Get the connection status of the API.
        /// </summary>
        public ConnectionStatus GetConnectionStatus() => this.APIClient.LastStatusCode == HttpStatusCode.TooManyRequests
                ? ConnectionStatus.Ratelimited
                : !this.hasConnectedSinceError
                ? ConnectionStatus.Error
                : this.APIClient.SSEIsConnected
                ? ConnectionStatus.Connected
                : this.APIClient.SSEIsConnecting ? ConnectionStatus.Connecting : ConnectionStatus.Disconnected;


        private void UpdateMetadataCache()
        {
            var metadata = this.APIClient.GetMetadata();
            if (metadata != null)
            {
                this.MetadataCache = metadata;
                PluginLog.Debug($"APIClientManager(GetMetadata): Metadata cache updated {JsonConvert.SerializeObject(this.MetadataCache)}");
            }
            else
            {
                PluginLog.Debug($"APIClientManager(GetMetadata): Unable to update metadata cache.");
            }
        }
    }
}
