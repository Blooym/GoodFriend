using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Plugin;
using GoodFriend.Plugin.Configuration;
using GoodFriend.Plugin.Managers;
using GoodFriend.Plugin.Resources.Localization;
using GoodFriend.Plugin.UserInterface;
using Sirensong.IoC;

namespace GoodFriend.Plugin.Common
{
    /// <summary>
    ///     Provides access to necessary instances and services.
    /// </summary>
    internal sealed class Services
    {
        /// <inheritdoc cref="MiniServiceContainer" />
        private static readonly MiniServiceContainer ServiceContainer = new();

        // Dalamud services
        [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ClientState ClientState { get; private set; } = null!;
        [PluginService] internal static ChatGui Chat { get; private set; } = null!;
        [PluginService] internal static ToastGui Toast { get; private set; } = null!;
        [PluginService] internal static Condition Condition { get; private set; } = null!;
        [PluginService] internal static Framework Framework { get; private set; } = null!;


        internal static WindowManager WindowManager { get; private set; } = null!;
        internal static APIClientManager APIClientManager { get; private set; } = null!;
        internal static PluginConfiguration Configuration { get; private set; } = null!;

        /// <summary>
        ///     Initializes the service class.
        /// </summary>
        internal static void Initialize()
        {
            Configuration = PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            ServiceContainer.CreateService<LocalizationManager>();
            APIClientManager = ServiceContainer.GetOrCreateService<APIClientManager>();
            WindowManager = ServiceContainer.GetOrCreateService<WindowManager>();
        }

        /// <summary>
        ///     Disposes of the service class.
        /// </summary>
        internal static void Dispose() => ServiceContainer.Dispose();
    }
}
