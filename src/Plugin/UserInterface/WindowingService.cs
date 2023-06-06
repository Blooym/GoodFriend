using System;
using Dalamud.Interface.Windowing;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.UserInterface.Windows.MainWindow;

namespace GoodFriend.Plugin.UserInterface
{
    internal sealed class WindowingService : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        ///     The windowing system to use.
        /// </summary>
        private readonly WindowSystem windowSystem = new(Constants.PluginName);

        /// <summary>
        ///     All windows to add to the windowing system, holds all references.
        /// </summary>
        private readonly Window[] windows = {
            new MainWindow(),
        };

        /// <summary>
        ///     The configuration window.
        /// </summary>
        private Window ConfigWindow => this.windows[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowingService" /> class.
        /// </summary>
        public WindowingService()
        {
            foreach (var window in this.windows)
            {
                this.windowSystem.AddWindow(window);
            }

            DalamudInjections.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
            DalamudInjections.PluginInterface.UiBuilder.OpenConfigUi += this.ConfigWindow.Toggle;
        }

        /// <summary>
        ///     Disposes of the window manager.
        /// </summary>
        public void Dispose()
        {
            if (this.disposedValue)
            {
                return;
            }

            this.windowSystem.RemoveAllWindows();
            DalamudInjections.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
            DalamudInjections.PluginInterface.UiBuilder.OpenConfigUi -= this.ConfigWindow.Toggle;

            this.disposedValue = true;
        }
    }
}
