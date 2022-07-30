namespace GoodFriend;

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Threading.Tasks;
using GoodFriend.Managers;
using GoodFriend.UI.Settings;
using GoodFriend.Utils;
using GoodFriend.Base;

internal class KikoPlugin : IDalamudPlugin
{
    public string Name => PStrings.PluginName;
    private protected ulong _currentContentId;
    private protected APIClientManager _clientManager { get; init; }
    private protected SettingsScreen _settingsScreen { get; init; }

    public KikoPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Service.Initialize(Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration());

        this._clientManager = new APIClientManager();
        this._settingsScreen = new SettingsScreen();

        // If the user has started the plugin whilst already logged in, connect & cache the content id.
        if (Service.ClientState.LocalPlayer != null && !_clientManager.IsConnected)
        {
            this._clientManager.Connect();
            this._currentContentId = Service.ClientState.LocalContentId;
        }

        // Event handlers.
        _clientManager.DataRecieved += OnDataRecieved;
        Service.ClientState.Login += OnLogin;
        Service.ClientState.Logout += OnLogout;
        Service.PluginInterface.UiBuilder.Draw += DrawUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }


    /// <summary>
    ///     Event handler for when the plugin is told to draw the UI.
    /// </summary>
    private protected void DrawUI() => this._settingsScreen.Draw();


    /// <summary>
    ///     Event handler for when the UI is told to draw the config UI (Dalamud settings button)
    /// </summary>
    private protected void DrawConfigUI() => this._settingsScreen.visible = !this._settingsScreen.visible;


    /// <summary>
    ///     When the player logs in, connect to the events APi and send a login status update, and cache the current content id.
    /// </summary>
    private unsafe void OnLogin(object? sender, EventArgs e)
    {
        if (!this._clientManager.IsConnected) this._clientManager.Connect();

        Task.Run(() =>
        {
            while (Service.ClientState.LocalContentId == 0)
            {
                Task.Delay(100).Wait();
            }

            this._currentContentId = Service.ClientState.LocalContentId;

            this._clientManager.SendLogin(this._currentContentId);

            // Show the total amount of friends online when logging in.
            var onlineFriends = 0;
            foreach (var friend in FriendList.Get()) { if (friend->IsOnline) onlineFriends++; }
            this.Notify($"You have {onlineFriends} friends online.");
        });
    }


    /// <summary>
    ///     When the player logs out, disconnect from the events API and send a logout status update and clear the current content id.
    /// </summary>
    private void OnLogout(object? sender, EventArgs e)
    {
        if (this._clientManager.IsConnected) this._clientManager.Disconnect();
        _clientManager.SendLogout(this._currentContentId);
        this._currentContentId = 0;
    }


    /// <summary>
    ///     Handles data recieved from the ClientManager 
    /// </summary>
    private unsafe void OnDataRecieved(Update data)
    {
        FriendListEntry* friend = default;

        foreach (var x in FriendList.Get())
        {
            if (Hashing.Hash(x->ContentId.ToString()) == data.ContentID)
            {
                friend = x;
                break;
            }
        }

        if (friend == null) return;

        if (data.LoggedIn == true) this.Notify(string.Format(Service.Configuration.FriendLoggedInMessage, friend->Name.ToString()));
        else this.Notify(string.Format(Service.Configuration.FriendLoggedOutMessage, friend->Name.ToString()));
    }


    /// <summary>
    ///     Sends a notification to the user using their preferred notification type.
    /// </summary>
    private void Notify(string message)
    {
        if (Service.Configuration.NotificationType == NotificationType.Toast)
            Service.PluginInterface.UiBuilder.AddNotification(message, PStrings.PluginName);

        else if (Service.Configuration.NotificationType == NotificationType.Chat)
        {
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(35);
            stringBuilder.AddText(message);
            stringBuilder.AddUiForegroundOff();
            Service.Chat.Print(stringBuilder.BuiltString);
        }

        else if (Service.Configuration.NotificationType == NotificationType.Popup)
            Service.Toast.ShowNormal(message);
    }


    ///<summary>
    ///     Handles disposing of all resources used by the plugin.
    /// </summary>
    public void Dispose()
    {
        _clientManager.DataRecieved -= OnDataRecieved;
        Service.ClientState.Login -= OnLogin;
        Service.ClientState.Logout -= OnLogout;
        Service.PluginInterface.UiBuilder.Draw -= DrawUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
        _clientManager.Dispose();
    }
}
