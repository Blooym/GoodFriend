namespace GoodFriend.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using GoodFriend.Base;
using GoodFriend.UI.Screens.Settings;

/// <summary>
///     Initializes and manages all windows and window-events for the plugin.
/// </summary>
sealed public class WindowManager : IDisposable
{
    private readonly WindowSystem _windowSystem = new WindowSystem(PStrings.pluginName);

    private readonly List<Window> _windows = new()
    {
        new SettingsScreen(),
    };

    /// <summary>
    ///     Initializes the WindowManager and associated resources.
    /// </summary>
    public WindowManager()
    {
        PluginLog.Debug("WindowManager: Initializing...");

        foreach (var window in this._windows)
        {
            PluginLog.Debug($"WindowManager: Registering window: {window.WindowName}");
            this._windowSystem.AddWindow(window);
        }

        PluginService.PluginInterface.UiBuilder.Draw += OnDrawUI;
        PluginService.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

        PluginLog.Debug("WindowManager: Successfully initialized.");
    }

    /// <summary>
    ///     Draws all windows for the draw event.
    /// </summary>
    private void OnDrawUI() => _windowSystem.Draw();

    /// <summary>
    ///     Opens/Closes the plugin configuration screen. 
    /// </summary> 
    private void OnOpenConfigUI()
    {
        if (this._windowSystem.GetWindow($"{PStrings.pluginName} - {TStrings.SettingsTabSettings}") is SettingsScreen window)
            window.IsOpen = !window.IsOpen;
    }

    /// <summary>
    ///     Disposes of the WindowManager and associated resources.
    /// </summary>
    public void Dispose()
    {
        PluginLog.Debug("WindowManager: Disposing...");

        PluginService.PluginInterface.UiBuilder.Draw -= OnDrawUI;
        PluginService.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;

        foreach (var window in this._windows.OfType<IDisposable>())
        {
            PluginLog.Debug($"WindowManager: Disposing of {window.GetType().Name}...");
            window.Dispose();
        }

        this._windowSystem.RemoveAllWindows();

        PluginLog.Debug("WindowManager: Successfully disposed all windows.");
    }
}