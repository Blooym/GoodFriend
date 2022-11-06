using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using GoodFriend.Managers;

namespace GoodFriend.Base
{
    /// <summary>
    ///     Provides access to necessary instances and services.
    /// </summary>
#pragma warning disable CS8618 // Injection is handled by the Dalamud Plugin Framework here.
    internal sealed class PluginService
    {
        [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] internal static ClientState ClientState { get; private set; }
        [PluginService] internal static ChatGui Chat { get; private set; }
        [PluginService] internal static ToastGui Toast { get; private set; }
        [PluginService] internal static Condition Condition { get; private set; }
        [PluginService] internal static Framework Framework { get; private set; }

        internal static WindowManager WindowManager { get; private set; }
        internal static ResourceManager ResourceManager { get; private set; }
        internal static APIClientManager APIClientManager { get; private set; }
        internal static Configuration Configuration { get; private set; }
        internal static EventLogManager EventLogManager { get; private set; }

        /// <summary>
        ///     Initializes the service class.
        /// </summary>
        internal static void Initialize()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            ResourceManager = new ResourceManager();
            WindowManager = new WindowManager();
            APIClientManager = new APIClientManager(ClientState, Framework);
            EventLogManager = new EventLogManager();

#if !DEBUG
            ResourceManager.Update();
#endif

            EventLogManager.AddEntry("Services initialized successfully.", EventLogManager.EventLogType.Debug);
            PluginLog.Debug("PluginService(Initialize): Successfully initialized.");
        }

        /// <summary>
        ///     Disposes of the service class.
        /// </summary>
        internal static void Dispose()
        {
            WindowManager.Dispose();
            ResourceManager.Dispose();
            APIClientManager.Dispose();
        }
    }
}
