namespace GoodFriend.Managers
{
    using System;
    using System.Threading.Tasks;
    using System.Net;
    using System.Timers;
    using System.Net.Http;
    using GoodFriend.Base;
    using GoodFriend.Utils;
    using GoodFriend.Types;
    using GoodFriend.Enums;
    using Dalamud.Logging;
    using Dalamud.Game.ClientState;
    using ToastType = Dalamud.Interface.Internal.Notifications.NotificationType;
    using Newtonsoft.Json;

    /// <summary>
    ///     Manages an APIClient and its resources and handles events.
    /// </summary>
    public sealed class APIClientManager : IDisposable
    {
        /// <summary>
        ///    The current player ContentID, saved on login and cleared on logout.
        /// </summary>
        private ulong _currentContentId = 0;

        /// <summary>
        ///    The current player HomeworldID. Saved on login and cleared on logout.
        /// </summary>
        private uint _currentHomeworldId = 0;

        /// <summary>
        ///    The current player TerritoryID. Saved on login/zone change and cleared on logout.
        /// </summary>
        private uint _currentTerritoryId = 0;

        /// <summary>
        ///    The cached metadata from the API.
        /// </summary>
        public APIClient.MetadataPayload? metadataCache { get; private set; } = null;

        /// <summary>
        ///    The timer for updating the metadata cache.
        /// </summary>
        private Timer _metadataTimer = new Timer(240000);

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
        public string version => APIClient.apiVersion;

        /// <summary>
        ///     The ClientState associated with the manager
        /// </summary>
        private readonly ClientState _clientState;

        /// <summary>
        ///     Instantiates a new APIClientManager. 
        /// </summary>
        public APIClientManager(ClientState clientState)
        {
            PluginLog.Debug("APIClientManager(APIClientManager): Initializing...");

            this._clientState = clientState;

            // Create event handlers
            this.APIClient.SSEDataReceived += this.OnSSEDataReceived;
            this.APIClient.SSEConnectionError += this.OnSSEAPIClientError;
            this.APIClient.SSEConnectionEstablished += this.OnSSEAPIClientConnected;
            this.APIClient.SSEConnectionClosed += this.OnSSEAPIClientDisconnected;
            this.APIClient.RequestError += this.OnRequestError;
            this.APIClient.RequestSuccess += this.OnRequestSuccess;
            this._clientState.Login += this.OnLogin;
            this._clientState.Logout += this.OnLogout;
            this._clientState.TerritoryChanged += this.OnTerritoryChange;

            // Metadata cache update timer
            this._metadataTimer.Elapsed += this.OnMetadataTimerElapsed;
            this._metadataTimer.AutoReset = true;
            this._metadataTimer.Enabled = true;

            // If the player is logged in already, connect & set their ID.
            if (this._clientState.LocalPlayer != null)
            {
                this._currentContentId = this._clientState.LocalContentId;
                this._currentHomeworldId = this._clientState.LocalPlayer.HomeWorld.Id;
                this._currentTerritoryId = this._clientState.TerritoryType;
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
            this._clientState.Login -= this.OnLogin;
            this._clientState.Logout -= this.OnLogout;
            this._clientState.TerritoryChanged -= this.OnTerritoryChange;
            this.APIClient.SSEDataReceived -= this.OnSSEDataReceived;
            this.APIClient.SSEConnectionError -= this.OnSSEAPIClientError;
            this.APIClient.SSEConnectionEstablished -= this.OnSSEAPIClientConnected;
            this.APIClient.SSEConnectionClosed -= this.OnSSEAPIClientDisconnected;
            this.APIClient.RequestError -= this.OnRequestError;
            this.APIClient.RequestSuccess -= this.OnRequestSuccess;

            this._metadataTimer.Elapsed -= this.OnMetadataTimerElapsed;
            this._metadataTimer.Dispose();

            PluginLog.Debug("APIClientManager(Dispose): Successfully disposed.");
        }

        /// <summary>
        ///     Handles the login event by opening the APIClient stream and sending a login.
        /// </summary>
        private unsafe void OnLogin(object? sender, EventArgs e)
        {
            if (!this.APIClient.SSEIsConnected) this.APIClient.OpenSSEStream();
            Task.Run(() =>
            {
                while (this._clientState.LocalContentId == 0 || this._clientState.LocalPlayer == null)
                    Task.Delay(100).Wait();
                this._currentContentId = this._clientState.LocalContentId;
                this._currentHomeworldId = this._clientState.LocalPlayer.HomeWorld.Id;
                this._currentTerritoryId = this._clientState.TerritoryType;
                this.APIClient.SendLogin(this._currentContentId, this._currentHomeworldId, this._currentTerritoryId);
                PluginLog.Information($"APIClientManager(OnLogin): Stored IDs set to: Homeworld: {this._currentHomeworldId}, Content: [REDACTED], Territory: {this._currentTerritoryId}");
            });
        }

        /// <summary>
        ///     Handles the logout event by closing the APIClient stream and sending a logout.
        /// </summary>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (this.APIClient.SSEIsConnected) this.APIClient.CloseSSEStream();
            this.APIClient.CancelPendingRequests();
            this.APIClient.SendLogout(this._currentContentId, this._currentHomeworldId, this._currentTerritoryId);
            this._currentContentId = 0;
            this._currentHomeworldId = 0;
            this._currentTerritoryId = 0;
            this._hasConnectedSinceError = true;
            PluginLog.Information("APIClientManager(OnLogout): Player logged out, stored IDs reset to 0.");

        }

        /// <summary>
        ///     Handles the territory change event.
        /// </summary>
        private void OnTerritoryChange(object? sender, ushort newId)
        {
            if (this.APIClient.SSEIsConnected)
            {
                this._currentTerritoryId = newId;
                PluginLog.Debug($"APIClientManager(OnTerritoryChange): Stored TerritoryID changed from {this._currentTerritoryId} to {newId}.");
            }
        }

        /// <summary>
        ///     Handles data received from the APIClient.
        /// </summary> 
        private unsafe void OnSSEDataReceived(APIClient.UpdatePayload data)
        {
            PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Data received from API, processing.");

            // Don't process the data if the ContentID is null.
            if (data.ContentID == null)
            {
                PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Event was malformed due to a null ContentID, ignoring.");
                return;
            }

            // Find if the friend is on the users friend list.
            FriendListEntry* friend = default;
            foreach (var x in FriendList.Get())
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
            if (this._clientState.LocalPlayer?.CompanyTag?.ToString() != string.Empty
                && friend->FreeCompany.ToString() == this._clientState?.LocalPlayer?.CompanyTag.ToString()
                && friend->HomeWorld == this._clientState.LocalPlayer.HomeWorld.Id
                && PluginService.Configuration.HideSameFC)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to sharing the same free company & homeworld. (FC: {friend->FreeCompany})");
                return;
            }

            if (this._currentHomeworldId != friend->HomeWorld && PluginService.Configuration.HideDifferentHomeworld)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different homeworld. (Them: {friend->HomeWorld}, You: {this._currentHomeworldId})");
                return;
            }

            // If the event is not for the current territory, ignore it.
            if (data.TerritoryID != this._currentTerritoryId && PluginService.Configuration.HideDifferentTerritory)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being for a different territory. (Them: {data.TerritoryID}, You: {this._currentTerritoryId})");
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
        private bool _hasConnectedSinceError = true;

        /// <summary>
        ///     Handles the APIClient error event.
        /// </summary>
        private void OnSSEAPIClientError(Exception e)
        {
            this._hasConnectedSinceError = false;
            PluginLog.Error($"APIClientManager(OnSSEAPIClientError): A connection error occured: {e.Message}");
            PluginService.EventLogManager.AddEntry($"{Events.APIConnectionError}: {e.Message}", EventLogManager.EventLogType.Error);
            if (this._hasConnectedSinceError && PluginService.Configuration.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionError, NotificationType.Toast, ToastType.Error);
            }
        }

        /// <summary> 
        ///     Handles the APIClient connected event.
        /// </summary>
        private void OnSSEAPIClientConnected()
        {
            this._hasConnectedSinceError = true;
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
            this._hasConnectedSinceError = false;
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
        public ConnectionStatus GetConnectionStatus()
        {
            if (this.APIClient.LastStatusCode == HttpStatusCode.TooManyRequests) return ConnectionStatus.Ratelimited;
            if (!this._hasConnectedSinceError) return ConnectionStatus.Error;
            if (this.APIClient.SSEIsConnected) return ConnectionStatus.Connected;
            if (this.APIClient.SSEIsConnecting) return ConnectionStatus.Connecting;
            return ConnectionStatus.Disconnected;
        }


        private void UpdateMetadataCache()
        {
            var metadata = this.APIClient.GetMetadata();
            if (metadata != null)
            {
                this.metadataCache = metadata;
                PluginLog.Debug($"APIClientManager(GetMetadata): Metadata cache updated {JsonConvert.SerializeObject(this.metadataCache)}");
            }
            else PluginLog.Debug($"APIClientManager(GetMetadata): Unable to update metadata cache.");
        }
    }
}