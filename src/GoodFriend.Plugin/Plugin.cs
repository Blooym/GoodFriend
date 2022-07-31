namespace GoodFriend;

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Threading.Tasks;
using System.IO;
using CheapLoc;
using GoodFriend.Managers;
using GoodFriend.UI.Settings;
using GoodFriend.Utils;
using GoodFriend.Base;

public sealed class GoodFriendPlugin : IDalamudPlugin
{
    public string Name => PStrings.pluginName;
    private ulong _currentContentId;
    private APIClientManager _clientManager { get; init; }
    private SettingsScreen _settingsScreen { get; init; }

    public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Service.Initialize(Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration());
        OnLanguageChange(Service.PluginInterface.UiLanguage);

        this._clientManager = new APIClientManager();
        this._settingsScreen = new SettingsScreen();

        // If the user has started the plugin whilst already logged in, connect & cache the content id.
        if (Service.ClientState.LocalPlayer != null && !_clientManager.IsConnected)
        {
            this._clientManager.Connect();
            this._currentContentId = Service.ClientState.LocalContentId;
        }

        // Event handlers.
        this._clientManager.DataRecieved += OnDataRecieved;
        Service.ClientState.Login += OnLogin;
        Service.ClientState.Logout += OnLogout;
        Service.PluginInterface.LanguageChanged += OnLanguageChange;
        Service.PluginInterface.UiBuilder.Draw += DrawUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }


    /// <summary>
    ///     Event handler for when the plugin is told to draw the UI.
    /// </summary>
    private void DrawUI() => this._settingsScreen.Draw();


    /// <summary>
    ///     Event handler for when the UI is told to draw the config UI (Dalamud settings button)
    /// </summary>
    private void DrawConfigUI() => this._settingsScreen.visible = !this._settingsScreen.visible;


    /// <summary>
    ///     When the player logs in, connect to the events APi and send a login status update, and cache the current content id.
    /// </summary>
    private unsafe void OnLogin(object? sender, EventArgs e)
    {
        if (!this._clientManager.IsConnected) this._clientManager.Connect();

        Task.Run(() =>
        {
            while (Service.ClientState.LocalContentId == 0) Task.Delay(100).Wait();

            // Cache the contentID of the player and send a login status update.
            this._currentContentId = Service.ClientState.LocalContentId;
            this._clientManager.SendLogin(this._currentContentId);
        });
    }

    /// <summary>
    ///     When the player logs out, disconnect from the events API and send a logout status update and clear the current content id.
    /// </summary>
    private void OnLogout(object? sender, EventArgs e)
    {
        // Disconnect from the veents API and send a logout status update, and clear the content id.
        if (this._clientManager.IsConnected) this._clientManager.Disconnect();
        this._clientManager.SendLogout(this._currentContentId);
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
            Service.PluginInterface.UiBuilder.AddNotification(message, PStrings.pluginName);

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

    /// <summary>
    ///    Event handler for when the language is changed, reloads the localization strings.
    /// </summary>
    public static void OnLanguageChange(string language)
    {
        var uiLang = Service.PluginInterface.UiLanguage;
        try { Loc.Setup(File.ReadAllText($"{PStrings.localizationPath}\\{uiLang}.json")); }
        catch { Loc.SetupWithFallbacks(); }
    }

    ///<summary>
    ///     Handles disposing of all resources used by the plugin.
    /// </summary>
    public void Dispose()
    {
        this._clientManager.Dispose();
        this._clientManager.DataRecieved -= OnDataRecieved;
        Service.ClientState.Login -= OnLogin;
        Service.ClientState.Logout -= OnLogout;
        Service.PluginInterface.LanguageChanged -= OnLanguageChange;
        Service.PluginInterface.UiBuilder.Draw -= DrawUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
    }
}
