namespace GoodFriend.Managers;

using System;
using System.Threading.Tasks;
using Dalamud.Logging;
using Dalamud.Game.Text.SeStringHandling;
using GoodFriend.Base;
using GoodFriend.Utils;

sealed public class APINotifier : IDisposable
{

    /// <summary> The players current content ID for their login. </summary>
    private ulong _currentContentId = ulong.MinValue;

    /// <summary> THe API Client to get notified from. </summary>
    private APIClientManager _apiClient;

    public APINotifier(APIClientManager APIClient)
    {
        PluginLog.Debug("APINotifier: Initializing...");

        _apiClient = APIClient;

        _apiClient.DataRecieved += OnDataRecieved;
        PluginService.ClientState.Login += OnLogin;
        PluginService.ClientState.Logout += OnLogout;

        // If the player is already logged in when the plugin starts, connect to the events API and cache the content id.
        if (PluginService.ClientState.LocalPlayer != null)
        {
            _apiClient.Connect();
            this._currentContentId = PluginService.ClientState.LocalContentId;
        }

        PluginLog.Debug("APINotifier: Successfully initialized.");
    }


    /// <summary>
    ///  When the player logs in, connect to the events APi and send a login status update, and cache the current content id.
    /// </summary>
    private unsafe void OnLogin(object? sender, EventArgs e)
    {
        if (!_apiClient.IsConnected) _apiClient.Connect();

        Task.Run(() =>
        {
            while (PluginService.ClientState.LocalContentId == 0) Task.Delay(100).Wait();

            // Cache the contentID of the player and send a login status update.
            this._currentContentId = PluginService.ClientState.LocalContentId;
            _apiClient.SendLogin(this._currentContentId);
        });
    }


    /// <summary>
    ///  When the player logs out, disconnect from the events API and send a logout status update and clear the current content id.
    /// </summary>
    private void OnLogout(object? sender, EventArgs e)
    {
        // Disconnect from the veents API and send a logout status update, and clear the content id.
        if (_apiClient.IsConnected) _apiClient.Disconnect();
        _apiClient.SendLogout(this._currentContentId);
        this._currentContentId = 0;
    }


    /// <summary>
    ///  Handles data recieved from the ClientManager 
    /// </summary>
    private unsafe void OnDataRecieved(Update data)
    {
        FriendListEntry* friend = default;

        foreach (var x in FriendList.Get())
        {
            if (Hashing.HashSHA512(x->ContentId.ToString()) == data.ContentID)
            {
                friend = x;
                break;
            }
        }

        if (friend == null) return;

        if (friend->FreeCompany.ToString() == PluginService.ClientState?.LocalPlayer?.CompanyTag.ToString() && PluginService.Configuration.HideSameFC)
        {
            PluginLog.Debug($"Recieved update for {friend->Name} but ignored it due to sharing the same free company. (FC: {friend->FreeCompany})");
            return;
        }

        if (data.LoggedIn == true) this.Notify(string.Format(PluginService.Configuration.FriendLoggedInMessage, friend->Name.ToString()));
        else this.Notify(string.Format(PluginService.Configuration.FriendLoggedOutMessage, friend->Name.ToString()));
    }


    /// <summary>
    ///  Sends a notification to the user using their preferred notification type.
    /// </summary>
    private void Notify(string message)
    {
        if (PluginService.Configuration.NotificationType == NotificationType.Toast)
            PluginService.PluginInterface.UiBuilder.AddNotification(message, PStrings.pluginName);

        else if (PluginService.Configuration.NotificationType == NotificationType.Chat)
        {
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(35);
            stringBuilder.AddText(message);
            stringBuilder.AddUiForegroundOff();
            PluginService.Chat.Print(stringBuilder.BuiltString);
        }

        else if (PluginService.Configuration.NotificationType == NotificationType.Popup)
            PluginService.Toast.ShowNormal(message);
    }


    public void Dispose()
    {
        _apiClient.DataRecieved -= OnDataRecieved;
        PluginService.ClientState.Login -= OnLogin;
        PluginService.ClientState.Logout -= OnLogout;
    }
}