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
        private ulong _currentContentId;

        /// <summary>
        ///    The current player HomeworldID. Saved on login and cleared on logout.
        /// </summary>
        private uint _currentHomeworldId;

        /// <summary>
        ///    The current player WorldID. Saved on login/world change and cleared on logout.
        /// </summary>
        private uint _currentWorldId;

        /// <summary>
        ///    The current player DataCenterID. Saved on login/world change and cleared on logout.
        /// </summary>
        private uint _currentDatacenterId;

        /// <summary>
        ///    The current player TerritoryID. Saved on login/zone change and cleared on logout.
        /// </summary>
        private uint _currentTerritoryId;

        /// <summary>
        ///    The cached metadata from the API.
        /// </summary>
        public APIClient.MetadataPayload? MetadataCache { get; private set; }

        /// <summary>
        ///    The timer for updating the metadata cache.
        /// </summary>
        private readonly Timer _metadataTimer = new(240000);

        /// <summary>
        ///    The event that fires when the metadata timer elapses.
        /// </summary>
        private void OnMetadataTimerElapsed(object? source, ElapsedEventArgs e)
        {
            PluginLog.Verbose($"APIClientManager(OnMetadataRefresh): Metadata timer elapsed, updating metadata cache.");
            UpdateMetadataCache();
        }

        /// <summary>
        ///     The API Client associated with the manager.
        /// </summary>
        private APIClient APIClient { get; set; } = new APIClient(PluginService.Configuration);

        /// <summary>
        ///     The APIClient version string.
        /// </summary>
        public string Version => APIClient.apiVersion;

        /// <summary>
        ///     The ClientState associated with the manager
        /// </summary>
        private readonly ClientState _clientState;

        /// <summary>
        ///     The Framework associated with the manager
        /// </summary>
        private readonly Framework _framework;

        /// <summary>
        ///     Instantiates a new APIClientManager.
        /// </summary>
        public APIClientManager(ClientState clientState, Framework framework)
        {
            PluginLog.Debug("APIClientManager(APIClientManager): Initializing...");

            _clientState = clientState;
            _framework = framework;
            UpdateMetadataCache();

            // Create event handlers
            APIClient.SSEDataReceived += OnSSEDataReceived;
            APIClient.SSEConnectionError += OnSSEAPIClientError;
            APIClient.SSEConnectionEstablished += OnSSEAPIClientConnected;
            APIClient.SSEConnectionClosed += OnSSEAPIClientDisconnected;
            APIClient.RequestError += OnRequestError;
            APIClient.RequestSuccess += OnRequestSuccess;
            _clientState.Login += OnLogin;
            _clientState.Logout += OnLogout;
            _clientState.TerritoryChanged += OnTerritoryChange;
            _framework.Update += OnFrameworkUpdate;

            // Metadata cache update timer
            _metadataTimer.Elapsed += OnMetadataTimerElapsed;
            _metadataTimer.AutoReset = true;
            _metadataTimer.Enabled = true;

            // If the player is logged in already, connect & set their ID.
            if (_clientState.LocalPlayer != null)
            {
                _currentContentId = _clientState.LocalContentId;
                _currentHomeworldId = _clientState.LocalPlayer.HomeWorld.Id;
                _currentTerritoryId = _clientState.TerritoryType;
                _currentWorldId = _clientState.LocalPlayer.CurrentWorld.Id;
                _currentDatacenterId = _clientState.LocalPlayer.CurrentWorld.GameData?.DataCenter.Row ?? 0;
                PluginLog.Information("APIClientManager(APIClientManager): Player is already logged in, setting IDs and connecting to API.");
                APIClient.OpenSSEStream();
            }

            PluginLog.Debug("APIClientManager(APIClientManager): Successfully initialized.");
        }

        /// <summary>
        ///     Disposes of the APIClientManager and its resources.
        /// </summary>
        public void Dispose()
        {
            APIClient.Dispose();
            _clientState.Login -= OnLogin;
            _clientState.Logout -= OnLogout;
            _clientState.TerritoryChanged -= OnTerritoryChange;
            _framework.Update -= OnFrameworkUpdate;
            APIClient.SSEDataReceived -= OnSSEDataReceived;
            APIClient.SSEConnectionError -= OnSSEAPIClientError;
            APIClient.SSEConnectionEstablished -= OnSSEAPIClientConnected;
            APIClient.SSEConnectionClosed -= OnSSEAPIClientDisconnected;
            APIClient.RequestError -= OnRequestError;
            APIClient.RequestSuccess -= OnRequestSuccess;

            _metadataTimer.Elapsed -= OnMetadataTimerElapsed;
            _metadataTimer.Stop();
            _metadataTimer.Dispose();

            PluginLog.Debug("APIClientManager(Dispose): Successfully disposed.");
        }

        /// <summary>
        ///     Handles the login event by opening the APIClient stream and sending a login.
        /// </summary>
        private unsafe void OnLogin(object? sender, EventArgs e)
        {
            if (!APIClient.SSEIsConnected)
            {
                APIClient.OpenSSEStream();
            }

            _ = Task.Run(() =>
            {
                while (_clientState.LocalContentId == 0 || _clientState.LocalPlayer == null)
                {
                    Task.Delay(100).Wait();
                }

                _currentContentId = _clientState.LocalContentId;
                _currentHomeworldId = _clientState.LocalPlayer.HomeWorld.Id;
                _currentTerritoryId = _clientState.TerritoryType;
                _currentWorldId = _clientState.LocalPlayer.CurrentWorld.Id;
                _currentDatacenterId = _clientState.LocalPlayer.CurrentWorld?.GameData?.DataCenter.Row ?? 0;
                APIClient.SendLogin(_currentContentId, _currentHomeworldId, _currentWorldId, _currentTerritoryId, _currentDatacenterId);
                PluginLog.Information($"APIClientManager(OnLogin): Stored IDs set to: Homeworld: {_currentHomeworldId}, World: {_currentWorldId}, Datacenter: {_currentDatacenterId}, Content: [REDACTED], Territory: {_currentTerritoryId}");
            });
        }

        /// <summary>
        ///     Handles the logout event by closing the APIClient stream and sending a logout.
        /// </summary>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (APIClient.SSEIsConnected)
            {
                APIClient.CloseSSEStream();
            }

            APIClient.CancelPendingRequests();
            APIClient.SendLogout(_currentContentId, _currentHomeworldId, _currentWorldId, _currentTerritoryId, _currentDatacenterId);
            _currentTerritoryId = 0;
            _currentWorldId = 0;
            _currentHomeworldId = 0;
            _currentContentId = 0;
            _currentDatacenterId = 0;
            _hasConnectedSinceError = true;
            PluginLog.Information("APIClientManager(OnLogout): Player logged out, stored IDs reset to 0.");

        }

        /// <summary>
        ///     Handles the territory change event.
        /// </summary>
        private void OnTerritoryChange(object? sender, ushort newId)
        {
            PluginLog.Debug($"APIClientManager(OnTerritoryChange): Stored TerritoryID changed from {_currentTerritoryId} to {newId}.");
            _currentTerritoryId = newId;
        }

        /// <summary>
        ///     Handles the framework update event.
        /// </summary>
        private void OnFrameworkUpdate(Framework framework)
        {
            Dalamud.Game.ClientState.Resolvers.ExcelResolver<Lumina.Excel.GeneratedSheets.World>? currentWorld = _clientState.LocalPlayer?.CurrentWorld;

            if (currentWorld != null && currentWorld.Id != 0 && currentWorld.Id != _currentWorldId)
            {
                PluginLog.Debug($"APIClientManager(OnFrameworkUpdate): Stored WorldID changed from {_currentWorldId} to {currentWorld.Id}.");
                _currentWorldId = currentWorld.Id;
            }
        }

        /// <summary>
        ///     Handles data received from the APIClient.
        /// </summary>
        private unsafe void OnSSEDataReceived(APIClient.UpdatePayload data)
        {
            if (data.ContentID == Hashing.HashSHA512(_currentContentId.ToString()))
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
                string hash = Hashing.HashSHA512(x->ContentId.ToString());
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
            if (_clientState.LocalPlayer?.CompanyTag?.ToString() != string.Empty
                && friend->FreeCompany.ToString() == _clientState?.LocalPlayer?.CompanyTag.ToString()
                && friend->HomeWorld == _clientState.LocalPlayer.HomeWorld.Id
                && PluginService.Configuration.HideSameFC)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to sharing the same free company & homeworld. (FC: {friend->FreeCompany})");
                return;
            }

            if (_currentHomeworldId != friend->HomeWorld && PluginService.Configuration.HideDifferentHomeworld)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different homeworld. (Them: {friend->HomeWorld}, You: {_currentHomeworldId})");
                return;
            }

            if (_currentWorldId != data.WorldID && PluginService.Configuration.HideDifferentWorld && data.WorldID != null && data.WorldID != 0)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different world. (Them: {data.WorldID}, You: {_currentWorldId})");
                return;
            }

            if (_currentDatacenterId != data.DatacenterID && PluginService.Configuration.HideDifferentDatacenter)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being on a different datacenter. (Them: {data.DatacenterID}, You: {_currentDatacenterId})");
                return;
            }

            // If the event is not for the current territory, ignore it.
            if (data.TerritoryID != _currentTerritoryId && PluginService.Configuration.HideDifferentTerritory)
            {
                PluginLog.Information($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received ignored due to being for a different territory. (Them: {data.TerritoryID}, You: {_currentTerritoryId})");
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
            _hasConnectedSinceError = false;
            PluginLog.Error($"APIClientManager(OnSSEAPIClientError): A connection error occured: {e.Message}");
            PluginService.EventLogManager.AddEntry($"{Events.APIConnectionError}: {e.Message}", EventLogManager.EventLogType.Error);
            if (_hasConnectedSinceError && PluginService.Configuration.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionError, NotificationType.Toast, ToastType.Error);
            }
        }

        /// <summary>
        ///     Handles the APIClient connected event.
        /// </summary>
        private void OnSSEAPIClientConnected()
        {
            _hasConnectedSinceError = true;
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
            _hasConnectedSinceError = false;
            PluginLog.Information($"APIClientManager(OnSSEAPIClientDisconnected): Disconnected from the API.");
            PluginService.EventLogManager.AddEntry(Events.APIConnectionDisconnected, EventLogManager.EventLogType.Info);
        }

        /// <summary>
        ///     Handles the APIClient RequestError event.
        /// </summary>
        private void OnRequestError(Exception e, HttpResponseMessage? response)
        {
            string? uri = response?.RequestMessage?.RequestUri?.ToString().Split('?')[0];

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
            string? uri = response.RequestMessage?.RequestUri?.ToString().Split('?')[0];
            PluginLog.Debug($"APIClientManager(OnRequestSuccess): Request to {uri} was successful with status code {response.StatusCode}.");
            PluginService.EventLogManager.AddEntry($"{response.StatusCode} ({uri})", EventLogManager.EventLogType.Debug);
        }

        /// <summary>
        ///     Get the connection status of the API.
        /// </summary>
        public ConnectionStatus GetConnectionStatus()
        {
            return APIClient.LastStatusCode == HttpStatusCode.TooManyRequests
                ? ConnectionStatus.Ratelimited
                : !_hasConnectedSinceError
                ? ConnectionStatus.Error
                : APIClient.SSEIsConnected
                ? ConnectionStatus.Connected
                : APIClient.SSEIsConnecting ? ConnectionStatus.Connecting : ConnectionStatus.Disconnected;
        }


        private void UpdateMetadataCache()
        {
            APIClient.MetadataPayload? metadata = APIClient.GetMetadata();
            if (metadata != null)
            {
                MetadataCache = metadata;
                PluginLog.Debug($"APIClientManager(GetMetadata): Metadata cache updated {JsonConvert.SerializeObject(MetadataCache)}");
            }
            else
            {
                PluginLog.Debug($"APIClientManager(GetMetadata): Unable to update metadata cache.");
            }
        }
    }
}
