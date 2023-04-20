using System;
using System.Collections.Generic;
using Dalamud.Interface.Windowing;
using GoodFriend.Plugin.UserInterface.Windows.Main;
using Sirensong;
using Sirensong.UserInterface.Windowing;

namespace GoodFriend.Plugin.UserInterface
{
    internal sealed class WindowManager : IDisposable
    {
        /// <summary>
        ///     All windows to add to the windowing system, holds all references.
        /// </summary>
        private readonly Dictionary<Window, bool> windows = new() { { new MainWindow(), true }, };

        /// <summary>
        ///     The windowing system.
        /// </summary>
        public WindowingSystem WindowingSystem { get; } = SirenCore.GetOrCreateService<WindowingSystem>();

        private bool disposedValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowManager" /> class.
        /// </summary>
        private WindowManager()
        {
            foreach (var (window, isSettings) in this.windows)
            {
                this.WindowingSystem.AddWindow(window, isSettings);
            }
        }

        /// <summary>
        ///     Disposes of the window manager.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposedValue)
            {
                this.WindowingSystem.Dispose();
                this.disposedValue = true;
            }
        }
    }
}