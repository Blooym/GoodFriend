namespace GoodFriend.Managers;

using System;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.UI.Screens.Settings;

/// <summary> Initializes and manages all windows and window-events for the plugin. </summary>
sealed public class WindowManager : IDisposable
{
    public readonly SettingsScreen Settings = new SettingsScreen();

    /// <summary> Draws all windows for the draw event. </summary>
    private void OnDraw() => Settings.Draw();


    /// <summary> Opens/Closes the plugin configuration screen. </summary> 
    private void OnOpenConfigUI() => Settings.presenter.isVisible = !Settings.presenter.isVisible;


    /// <summary> Initializes the WindowManager and associated resources. </summary>
    public WindowManager()
    {
        PluginLog.Debug("WindowManager: Initializing...");

        PluginService.PluginInterface.UiBuilder.Draw += OnDraw;
        PluginService.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

        PluginLog.Debug("WindowManager: Successfully initialized.");
    }


    /// <summary> Disposes of the WindowManager and associated resources. </summary>
    public void Dispose()
    {
        PluginLog.Debug("WindowManager: Disposing...");

        PluginService.PluginInterface.UiBuilder.Draw -= OnDraw;
        PluginService.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;

        Settings.Dispose();

        PluginLog.Debug("WindowManager: Successfully disposed.");
    }
}