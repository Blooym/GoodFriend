namespace GoodFriend.UI.Screens.Settings;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dalamud.Interface.ImGuiFileDialog;
using CheapLoc;
using GoodFriend.Base;
using GoodFriend.Managers;

sealed public class SettingsPresenter : IDisposable
{
    public void Dispose() { }


    /////////////////////////////
    ///   APIClient Log Data  ///
    /////////////////////////////

    public List<FriendStatusEvent> FetchAPILog() => PluginService.APIClientManager.GetLog();


    /////////////////////////////
    /// Reconnection  Methods ///
    /////////////////////////////

    /// <summary>
    ///     The cooldown before a user can reconnect to the API.
    /// </summary>
    private int _reconnectCooldown = 30;

    /// <summary>
    ///     If the reconnect cooldown is current active or not.
    /// </summary>
    public bool reconnectCooldownActive = false;

    /// <summary>
    ///     Attempts to reconnect to the API.
    /// </summary>
    public void ReconnectWithCooldown()
    {
        if (this.reconnectCooldownActive || PluginService.APIClientManager.APIClient.IsConnected) return;

        PluginService.APIClientManager.APIClient.OpenStream();

        this.reconnectCooldownActive = true;
        Task.Run(() =>
        {
            Thread.Sleep(this._reconnectCooldown * 1000);
            this.reconnectCooldownActive = false;
        });
    }


    /////////////////////////////
    ///     Other Methods     ///
    /////////////////////////////

    /// <summary> 
    /// Gets the current API client count.
    /// </summary>
    public int APIClients => PluginService.APIClientManager.APIClient.ConnectedClients;

    /// <summary>
    ///     Gets the current API connection status.
    /// </summary>
    public bool APIIsConnected => PluginService.APIClientManager.APIClient.IsConnected;

    /// <summary>
    ///     Should the reconnect button be disabled or not.
    /// </summary>
    public bool ReconnectButtonDisabled => this.reconnectCooldownActive || PluginService.ClientState.LocalPlayer == null || this.APIIsConnected;


#if DEBUG
    /// <summary>
    ///     The dialog manager for the settings window.
    /// </summary>
    public FileDialogManager dialogManager = new FileDialogManager();

    /// <summary>
    ///     The callback for when the user selects an export directory. 
    /// </summary>
    public void OnDirectoryPicked(bool success, string path)
    {
        if (!success) return;
        var directory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(path);
        Loc.ExportLocalizable();
        File.Copy(Path.Combine(path, "GoodFriend_Localizable.json"), Path.Combine(path, "en.json"), true);
        Directory.SetCurrentDirectory(directory);
        PluginService.PluginInterface.UiBuilder.AddNotification("Localization exported successfully.", PStrings.pluginName, Dalamud.Interface.Internal.Notifications.NotificationType.Success);
    }
#endif
}
