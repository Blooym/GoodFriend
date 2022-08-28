namespace GoodFriend.Managers;

using System;
using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using GoodFriend.Base;
using GoodFriend.UI.Screens.Settings;

/// <summary>
///     Initializes and manages all windows and window-events for the plugin.
/// </summary>
sealed public class WindowManager : IDisposable
{
    public WindowSystem WindowSystem = new WindowSystem("GoodFriend");

    /// <summary>
    ///     Draws all windows for the draw event.
    /// </summary>
    private void OnDraw() => WindowSystem.Draw();

    /// <summary>
    ///     Opens/Closes the plugin configuration screen. 
    /// </summary> 
    private void OnOpenConfigUI()
    {
        if (WindowSystem.GetWindow(PStrings.pluginName) is SettingsScreen window) window.IsOpen = !window.IsOpen;
    }

    /// <summary>
    ///     Initializes the WindowManager and associated resources.
    /// </summary>
    public WindowManager()
    {
        PluginLog.Debug("WindowManager: Initializing...");

        WindowSystem.AddWindow(new SettingsScreen());
        PluginService.PluginInterface.UiBuilder.Draw += OnDraw;
        PluginService.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

        PluginLog.Debug("WindowManager: Successfully initialized.");
    }

    /// <summary>
    ///     Disposes of the WindowManager and associated resources.
    /// </summary>
    public void Dispose()
    {
        PluginLog.Debug("WindowManager: Disposing...");

        this.WindowSystem.RemoveAllWindows();
        PluginService.PluginInterface.UiBuilder.Draw -= OnDraw;
        PluginService.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;

        PluginLog.Debug("WindowManager: Successfully disposed.");
    }
}