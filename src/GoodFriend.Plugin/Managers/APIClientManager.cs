namespace GoodFriend.Managers;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dalamud.Logging;
using ToastType = Dalamud.Interface.Internal.Notifications.NotificationType;
using Dalamud.Game.ClientState;
using GoodFriend.Base;
using GoodFriend.Utils;
using GoodFriend.Types;

/// <summary>
///     A friend status update that structures all stored notifications.
/// </summary>
sealed public class FriendStatusEvent
{
    public Guid EventID { get; set; }
    public FriendListEntry Friend { get; set; }
    public object? Event { get; set; }
    public DateTime Time { get; set; }
}


/// <summary>
///     Manages an APIClient and its resources and handles events.
/// </summary>
sealed public class APIClientManager : IDisposable
{
    ////////////////////////////
    /// Properties n' Fields ///
    ////////////////////////////

    /// <summary>
    ///    The current player ContentID, saved on login and cleared on logout.
    /// </summary>
    private ulong _currentContentId = 0;

    /// <summary>
    ///    The current player homeworldID, saved on login and cleared on logout.
    /// </summary>

    /// <summary> 
    ///     The API Client associated with the manager.
    /// </summary>
    public APIClient APIClient { get; private set; } = new APIClient(PluginService.Configuration);

    /// <summary>
    ///     The ClientState associated with the manager
    /// </summary>
    private readonly ClientState _clientState;


    //////////////////////////////
    ///       Event  Log       ///
    //////////////////////////////

    /// <summary>
    ///     A log of the last 20 friend login/logout events.
    /// </summary>
    private List<FriendStatusEvent> EventLog = new List<FriendStatusEvent>();

    /// <summary>
    ///     Get the event log.
    /// </summary>
    public List<FriendStatusEvent> GetLog() => EventLog.Reverse<FriendStatusEvent>().ToList();

    /// <summary>
    ///     Clear the event log.
    /// </summary>
    private void ClearLog() => this.EventLog = Enumerable.Empty<FriendStatusEvent>().ToList();

    /// <summary>
    ///     Add an event to the event log.
    /// </summary>
    private void AddLog(FriendStatusEvent e)
    {
        PluginLog.Verbose($"APIClientManager: Adding event {e.EventID} to the log.");
        this.EventLog.Add(e);
        if (this.EventLog.Count > 20) this.EventLog.RemoveAt(0);
    }


    ////////////////////////////
    ///    Event Handlers    ///
    ////////////////////////////

    /// <summary>
    ///     Stores if the API has connected since the last time an error was shown.
    ///     If not, hide the error.
    /// </summary>
    private bool _hasConnectedSinceError = true;

    /// <summary>
    ///     Handles the APIClient error event.
    /// </summary>
    private void OnAPIClientError(Exception e)
    {
        if (_hasConnectedSinceError && PluginService.Configuration.ShowAPIEvents)
        {
            this._hasConnectedSinceError = false;
            Notifications.Show(TStrings.ConnectionError, NotificationType.Toast, ToastType.Error);
        }
    }

    /// <summary> 
    ///     Handles the APIClient connected event.
    /// </summary>
    private void OnAPIClientConnected()
    {
        if (PluginService.Configuration.ShowAPIEvents)
        {
            this._hasConnectedSinceError = true;
            Notifications.Show(TStrings.ConnectionSuccessful, NotificationType.Toast, ToastType.Success);
        }
    }


    ////////////////////////////
    /// Construct n' Dispose ///
    ////////////////////////////

    /// <summary>
    ///     Instantiates a new APINotifier. 
    /// </summary>
    public APIClientManager(ClientState clientState)
    {
        PluginLog.Debug("APIClientManager: Initializing...");

        this._clientState = clientState;

        // Create event handlers
        this.APIClient.DataRecieved += OnDataRecieved;
        this.APIClient.ConnectionError += OnAPIClientError;
        this.APIClient.ConnectionEstablished += OnAPIClientConnected;
        this._clientState.Login += OnLogin;
        this._clientState.Logout += OnLogout;

        // If the player is logged in already, connect & set their ID.
        if (this._clientState.LocalPlayer != null)
        {
            this._currentContentId = this._clientState.LocalContentId;
            this.APIClient.OpenStream();
        }

        PluginLog.Debug("APIClientManager: Successfully initialized.");
    }

    /// <summary>
    ///     Disposes of the APINotifier and its resources.
    /// </summary>
    public void Dispose()
    {
        this.APIClient.Dispose();
        this.APIClient.DataRecieved -= OnDataRecieved;
        this._clientState.Login -= OnLogin;
        this._clientState.Logout -= OnLogout;
    }


    ////////////////////////////
    ///    Event Handlers    /// 
    ////////////////////////////

    /// <summary>
    ///     Handles the login event by opening the APIClient stream and sending a login.
    /// </summary>
    private unsafe void OnLogin(object? sender, EventArgs e)
    {
        if (!this.APIClient.IsConnected) this.APIClient.OpenStream();

        Task.Run(() =>
        {
            while (this._clientState.LocalContentId == 0) Task.Delay(100).Wait();

            // Cache the contentID of the player and send a login status update.
            this._currentContentId = this._clientState.LocalContentId;
            this.APIClient.SendLoginStatechange(this._currentContentId, LoginState.LoggedIn);
        });
    }


    /// <summary>
    ///     Handles the logout event by closing the APIClient stream and sending a logout.
    /// </summary>
    private void OnLogout(object? sender, EventArgs e)
    {
        if (this.APIClient.IsConnected) this.APIClient.CloseStream();
        this.APIClient.SendLoginStatechange(this._currentContentId, LoginState.LoggedOut);
        this._currentContentId = 0;
        this.ClearLog();
    }


    /// <summary>
    ///     Handles data recieved from the APIClient.
    /// </summary> 
    private unsafe void OnDataRecieved(UpdatePayload data)
    {
        // Generate a unique ID for this event so we can identify it in logs.
        var eventID = Guid.NewGuid();

        PluginLog.Verbose($"APIClientManager: [{eventID}] Processing inbound event.");

        // Process the data recieved.
        FriendListEntry* friend = default;
        foreach (var x in FriendList.Get())
            if (Hashing.HashSHA512(x->ContentId.ToString()) == data.ContentID) { friend = x; break; }

        // Client does not have a friend with the given contentID/hashed contentID.
        if (friend == null || data == null)
        {
            PluginLog.Verbose($"APIClientManager: [{eventID}] No friend found from data, ignoring");
            return;
        }

        // Client has a friend with the given contentID, but shares a free company with them and asked to not be notified.
        if (friend->FreeCompany.ToString() == this._clientState?.LocalPlayer?.CompanyTag.ToString() && PluginService.Configuration.HideSameFC)
        {
            PluginLog.Debug($"APIClientManager: [{eventID}] Recieved update for {friend->Name} but ignored it due to sharing the same free company. (FC: {friend->FreeCompany})");
            return;
        }

        // Add the event to the log
        this.AddLog(new FriendStatusEvent()
        {
            EventID = eventID,
            Friend = *friend,
            Event = data.LoginState == LoginState.LoggedIn ? "Login" : "Logout",
            Time = DateTime.Now
        });

        // Notify the client 
        PluginLog.Debug($"APIClientManager: [{eventID}] Recieved update for {friend->Name}:{friend->HomeWorld}, notifying...");
        Notifications.ShowPreferred(data.LoginState == LoginState.LoggedIn ?
            string.Format(PluginService.Configuration.FriendLoggedInMessage, friend->Name, friend->FreeCompany)
            : string.Format(PluginService.Configuration.FriendLoggedOutMessage, friend->Name, friend->FreeCompany), ToastType.Info);
    }
}