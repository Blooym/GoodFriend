namespace GoodFriend.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GoodFriend.Base;
    using GoodFriend.UI.Windows.Main;
    using GoodFriend.UI.Windows.URLUpdateNag;
    using Dalamud.Logging;
    using Dalamud.Interface.Windowing;

    /// <summary>
    ///     Initializes and manages all windows and window-events for the plugin.
    /// </summary>
    internal sealed class WindowManager : IDisposable
    {
        private readonly WindowSystem _windowSystem = new WindowSystem(PStrings.pluginName);

        private readonly List<Window> _windows = new()
        {
            new MainWindow(),
            new URLUpdateNagWindow(),
        };

        /// <summary>
        ///     Initializes the WindowManager and associated resources.
        /// </summary>
        internal WindowManager()
        {
            PluginLog.Debug("WindowManager(WindowManager): Initializing...");


            foreach (var window in this._windows)
            {
                PluginLog.Debug($"WindowManager(WindowManager): Registering window: {window.WindowName}");
                this._windowSystem.AddWindow(window);
            }

            PluginService.PluginInterface.UiBuilder.Draw += OnDrawUI;
            PluginService.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

            PluginLog.Debug("WindowManager(WindowManager): Successfully initialized.");
        }

        /// <summary>
        ///     Draws all windows for the draw event.
        /// </summary>
        private void OnDrawUI() => _windowSystem.Draw();

        /// <summary>
        ///     Opens/Closes the plugin configuration window. 
        /// </summary> 
        private void OnOpenConfigUI()
        {
            if (this._windowSystem.GetWindow(PStrings.pluginName) is MainWindow window)
                window.IsOpen = !window.IsOpen;
        }

        /// <summary>
        ///     Disposes of the WindowManager and associated resources.
        /// </summary>
        public void Dispose()
        {
            PluginLog.Debug("WindowManager(Dispose): Disposing...");

            PluginService.PluginInterface.UiBuilder.Draw -= OnDrawUI;
            PluginService.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;

            foreach (var window in this._windows.OfType<IDisposable>())
            {
                PluginLog.Debug($"WindowManager(Dispose): Disposing of {window.GetType().Name}...");
                window.Dispose();
            }

            this._windowSystem.RemoveAllWindows();

            PluginLog.Debug("WindowManager(Dispose): Successfully disposed all windows.");
        }
    }
}