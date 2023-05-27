using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.UI.Windows.Main;
using GoodFriend.UI.Windows.URLUpdateNag;

namespace GoodFriend.Managers
{
    /// <summary>
    ///     Initializes and manages all windows and window-events for the plugin.
    /// </summary>
    internal sealed class WindowManager : IDisposable
    {
        private readonly WindowSystem windowSystem = new(PluginConstants.PluginName);

        private readonly List<Window> windows = new()
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

            foreach (var window in this.windows)
            {
                PluginLog.Debug($"WindowManager(WindowManager): Registering window: {window.GetType().Name}");
                this.windowSystem.AddWindow(window);
            }

            PluginService.PluginInterface.UiBuilder.Draw += this.OnDrawUI;
            PluginService.PluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUI;

            PluginLog.Debug("WindowManager(WindowManager): Successfully initialized.");
        }

        /// <summary>
        ///     Draws all windows for the draw event.
        /// </summary>
        private void OnDrawUI() => this.windowSystem.Draw();

        /// <summary>
        ///     Opens/Closes the plugin configuration window.
        /// </summary>
        private void OnOpenConfigUI()
        {
            if (this.windows.FirstOrDefault(window => window is MainWindow) is MainWindow window)
            {
                window.IsOpen = !window.IsOpen;
            }
        }

        /// <summary>
        ///     Disposes of the WindowManager and associated resources.
        /// </summary>
        public void Dispose()
        {
            PluginService.PluginInterface.UiBuilder.Draw -= this.OnDrawUI;
            PluginService.PluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUI;

            foreach (var window in this.windows.OfType<IDisposable>())
            {
                PluginLog.Debug($"WindowManager(Dispose): Disposing of {window.GetType().Name}...");
                window.Dispose();
            }

            this.windowSystem.RemoveAllWindows();

            PluginLog.Debug("WindowManager(Dispose): Successfully disposed.");
        }
    }
}
