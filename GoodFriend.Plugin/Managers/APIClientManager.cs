using System;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game;
using Dalamud.Logging;
using GoodFriend.Client;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Common;
using GoodFriend.Plugin.Game;
using GoodFriend.Plugin.Localization;
using Sirensong.Game;
using ToastType = Dalamud.Interface.Internal.Notifications.NotificationType;

namespace GoodFriend.Plugin.Managers
{
    /// <summary>
    ///     Manages an APIClient and its resources and handles events.
    /// </summary>
    internal sealed class APIClientManager : IDisposable
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
        public MetadataResponse? MetadataCache { get; private set; }

        /// <summary>
        ///    The timer for updating the metadata cache.
        /// </summary>
        private readonly Timer metadataTimer = new(240000);

        /// <summary>
        ///    The event that fires when the metadata timer elapses.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMetadataTimerElapsed(object? source, ElapsedEventArgs e) => this.UpdateMetadataCache();

        /// <summary>
        ///     The API Client associated with the manager.
        /// </summary>
        private readonly GoodFriendClient apiClient = new(new Uri("http://127.0.0.1:8000"));

        public EventStreamConnectionState ConnectionStatus => this.apiClient.ConnectionState;

        /// <summary>
        ///     Instantiates a new APIClientManager.
        /// </summary>
        public APIClientManager()
        {
            this.UpdateMetadataCache();

            // Create event handlers
            this.apiClient.OnEventStreamStateUpdate += this.OnSSEDataReceived;
            this.apiClient.OnEventStreamException += this.OnSSEAPIClientError;
            this.apiClient.OnEventStreamConnected += this.OnSSEAPIClientConnected;
            this.apiClient.OnEventStreamDisconnected += this.OnSSEAPIClientDisconnected;
            Services.ClientState.Login += this.OnLogin;
            Services.ClientState.Logout += this.OnLogout;
            Services.Framework.Update += this.OnFrameworkUpdate;

            // Metadata cache update timer
            this.metadataTimer.Elapsed += this.OnMetadataTimerElapsed;
            this.metadataTimer.AutoReset = true;
            this.metadataTimer.Enabled = true;

            // If the player is logged in already, connect & set their ID.
            if (Services.ClientState.LocalPlayer != null)
            {
                this.currentContentId = Services.ClientState.LocalContentId;
                this.currentHomeworldId = Services.ClientState.LocalPlayer.HomeWorld.Id;
                this.currentTerritoryId = Services.ClientState.TerritoryType;
                this.currentWorldId = Services.ClientState.LocalPlayer.CurrentWorld.Id;
                this.currentDatacenterId = Services.ClientState.LocalPlayer.CurrentWorld.GameData?.DataCenter.Row ?? 0;
                PluginLog.Debug("APIClientManager(APIClientManager): Player is already logged in, setting IDs and connecting to API.");
                this.apiClient.ConnectToEventStream();
            }

            var motd = this.apiClient.GetMotd();
            if (!motd.Ignore)
            {
                if (motd.Urgent)
                {
                    GameChat.Print($"Important: {motd.Content}");
                }
                else
                {
                    GameChat.Print(motd.Content);
                }
                PluginLog.Debug("APIClientManager(APIClientManager): Successfully initialized.");
            }
        }

        /// <summary>
        ///     Disposes of the APIClientManager and its resources.
        /// </summary>
        public void Dispose()
        {
            this.apiClient.Dispose();
            Services.ClientState.Login -= this.OnLogin;
            Services.ClientState.Logout -= this.OnLogout;
            Services.Framework.Update -= this.OnFrameworkUpdate;
            this.apiClient.OnEventStreamStateUpdate -= this.OnSSEDataReceived;
            this.apiClient.OnEventStreamException -= this.OnSSEAPIClientError;
            this.apiClient.OnEventStreamConnected -= this.OnSSEAPIClientConnected;
            this.apiClient.OnEventStreamDisconnected -= this.OnSSEAPIClientDisconnected;

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
            if (this.apiClient.ConnectionState is EventStreamConnectionState.Disconnected or EventStreamConnectionState.Disconnecting or EventStreamConnectionState.Exception)
            {
                this.apiClient.ConnectToEventStream();
            }

            Task.Run(() =>
          {
              while (Services.ClientState.LocalContentId == 0 || Services.ClientState.LocalPlayer == null)
              {
                  Task.Delay(100).Wait();
              }

              // Set the current IDs
              this.currentContentId = Services.ClientState.LocalContentId;
              this.currentHomeworldId = Services.ClientState.LocalPlayer.HomeWorld.Id;
              this.currentTerritoryId = Services.ClientState.TerritoryType;
              this.currentWorldId = Services.ClientState.LocalPlayer.CurrentWorld.Id;
              this.currentDatacenterId = Services.ClientState.LocalPlayer.CurrentWorld?.GameData?.DataCenter.Row ?? 0;

              // Send a login
              this.apiClient.SendLogin(
                new LoginRequest()
                {
                    ContentIdHash = this.currentContentId.ToString(),
                    ContentIdSalt = "123",
                    DatacenterId = this.currentDatacenterId,
                    WorldId = this.currentWorldId,
                    TerritoryId = this.currentTerritoryId,

                });
              PluginLog.Debug("APIClientManager(OnLogin): successfully set stored IDs for the APIClientManager.");
          });
        }

        /// <summary>
        ///     Handles the logout event by closing the APIClient stream and sending a logout.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLogout(object? sender, EventArgs e)
        {
            if (this.apiClient.ConnectionState is EventStreamConnectionState.Connected or EventStreamConnectionState.Connecting)
            {
                this.apiClient.DisconnectFromEventStream();
            }

            // Cancel any pending requests and send a logout.
            this.apiClient.SendLogout(
                new LogoutRequest()
                {
                    ContentIdHash = this.currentContentId.ToString(),
                    ContentIdSalt = "123",
                    DatacenterId = this.currentDatacenterId,
                    WorldId = this.currentWorldId,
                    TerritoryId = this.currentTerritoryId,

                });

            // Clear the current IDs
            this.currentTerritoryId = 0;
            this.currentWorldId = 0;
            this.currentHomeworldId = 0;
            this.currentContentId = 0;
            this.currentDatacenterId = 0;
            this.hasConnectedSinceError = true;

            PluginLog.Debug("APIClientManager(OnLogout): successfully cleared stored IDs for the APIClientManager.");
        }

        /// <summary>
        ///     Handles the framework update event.
        /// </summary>
        /// <param name="framework"></param>
        private void OnFrameworkUpdate(Framework framework)
        {
            var currentWorld = Services.ClientState.LocalPlayer?.CurrentWorld;
            var currentTerritory = Services.ClientState.TerritoryType;

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
        private unsafe void OnSSEDataReceived(object? sender, EventStreamPlayerUpdate data)
        {
            // // Don't process the data if the ContentID is null.
            // if (data.ContentIdHash == null)
            // {
            //     PluginLog.Verbose("APIClientManager(OnSSEDataReceived): Event was malformed due to a null ContentID, ignoring.");
            //     return;
            // }

            // // Don't process the data if the hash matches the current player's hash.
            // if (data.ContentIdHash == CryptoUtil.HashSHA512(this.currentContentId.ToString(), data.ContentIdSalt ?? string.Empty))
            // {
            //     PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): ContentID is the same as the current character's ContentID, skipping.");
            //     return;
            // }

            // PluginLog.Verbose($"APIClientManager(OnSSEDataReceived): Data received from API, processing it ({data.ContentIdHash[..8]}...).");

            // // Find if the friend is on the users friend list.
            // FriendListEntry* friend = default;
            // foreach (FriendListEntry* x in FriendListManager.Get())
            // {
            //     var hash = CryptoUtil.HashSHA512(x->ContentId.ToString(), data.ContentIdSalt ?? string.Empty);
            //     if (hash == data.ContentIdHash)
            //     {
            //         friend = x;
            //         break;
            //     }
            // }

            // // If friend is null, the user is not added by the client or the ID is invalid.
            // if (friend == null)
            // {
            //     PluginLog.Verbose("APIClientManager(OnSSEDataReceived): No friend found from the event, cannot display to client.");
            //     return;
            // }

            // // If the player has "Hide Same FC" enabled, check if the FC matches the current player's FC.
            // if (Services.ClientState.LocalPlayer?.CompanyTag?.ToString() != string.Empty
            //     && friend->FreeCompany.ToString() == Services.ClientState?.LocalPlayer?.CompanyTag.ToString()
            //     && friend->HomeWorld == Services.ClientState.LocalPlayer.HomeWorld.Id
            //     && Services.Configuration.EventStreamUpdateFilterConfig.HideSameFC)
            // {
            //     PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to sharing the same free company & homeworld. (FC: {friend->FreeCompany})");
            //     return;
            // }

            // // If the player has "Hide Different Homeworld" enabled, check if the home world matches the current player's home world.
            // if (this.currentHomeworldId != friend->HomeWorld && Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentHomeworld)
            // {
            //     PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being on a different homeworld. (Them: {friend->HomeWorld}, You: {this.currentHomeworldId})");
            //     return;
            // }

            // //// If the player has "Hide Different World" enabled, check if the world matches the current player's world.
            // if (this.currentWorldId != data.WorldId && Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentWorld && data.WorldId != 0)
            // {
            //     PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being on a different world. (Them: {data.WorldId}, You: {this.currentWorldId})");
            //     return;
            // }

            // // If the player has "Hide Different Datacenter" enabled, check if the datacenter matches the current player's datacenter.
            // if (this.currentDatacenterId != data.DatacenterId && Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentDatacenter)
            // {
            //     PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being on a different datacenter. (Them: {data.DatacenterId}, You: {this.currentDatacenterId})");
            //     return;
            // }

            // // If the player has "Hide Different Territory" enabled, check if the territory matches the current player's territory.
            // if (data.TerritoryId != this.currentTerritoryId && Services.Configuration.EventStreamUpdateFilterConfig.HideDifferentTerritory)
            // {
            //     PluginLog.Debug($"APIClientManager(OnSSEDataReceived): Event for {friend->Name} received; ignoring due to being for a different territory. (Them: {data.TerritoryId}, You: {this.currentTerritoryId})");
            //     return;
            // }

            // // Everything is good, send the event to the player and add it to the log.
            // PluginLog.Debug($"APIClientManager(OnSSEDataReceived): {friend->Name} {(data.IsLoggedIn ? Events.LoggedIn : Events.LoggedOut)}");
            // Notifications.ShowPreferred(data.IsLoggedIn ?
            // string.Format(Services.Configuration.EventStreamUpdateMessagesConfig.FriendLoggedInMessage, friend->Name, friend->FreeCompany)
            // : string.Format(Services.Configuration.EventStreamUpdateMessagesConfig.FriendLoggedOutMessage, friend->Name, friend->FreeCompany), ToastType.Info);
        }

        /// <summary>
        ///     Stores if the API has connected since the last time an error was shown.
        /// </summary>
        private bool hasConnectedSinceError = true;

        /// <summary>
        ///     Handles the APIClient error event.
        /// </summary>
        /// <param name="e">The error.</param>
        private void OnSSEAPIClientError(object? sender, Exception e)
        {
            if (this.hasConnectedSinceError && Services.Configuration.ApiConfig.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionError, NotificationType.Toast, ToastType.Error);
            }
            this.hasConnectedSinceError = false;
        }

        /// <summary>
        ///     Handles the APIClient connected event.
        /// </summary>
        private void OnSSEAPIClientConnected(object? sender)
        {
            if (Services.Configuration.ApiConfig.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionSuccess, NotificationType.Toast, ToastType.Success);
            }
            this.hasConnectedSinceError = true;
        }

        /// <summary>
        ///     Handles the APIClient disconnected event.
        /// </summary>
        private void OnSSEAPIClientDisconnected(object? sender)
        {
            if (Services.Configuration.ApiConfig.ShowAPIEvents)
            {
                Notifications.Show(Events.APIConnectionDisconnected, NotificationType.Toast, ToastType.Info);
            }
        }

        private void UpdateMetadataCache() => this.MetadataCache = this.apiClient.GetMetadata();
    }
}
