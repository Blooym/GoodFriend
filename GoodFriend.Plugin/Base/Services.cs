using Dalamud.Plugin;
using GoodFriend.Plugin.Api;
using GoodFriend.Plugin.Configuration;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.UserInterface;

namespace GoodFriend.Plugin.Base
{
    /// <summary>
    ///     Provides access to necessary instances and services.
    /// </summary>
    internal static class Services
    {
        public static PluginConfiguration PluginConfiguration { get; private set; } = null!;
        public static LocalizationService LocalizationService { get; private set; } = null!;
        public static WindowingService WindowingService { get; private set; } = null!;
        public static RelayApiService ApiModuleService { get; private set; } = null!;

        /// <summary>
        ///     Initializes the service class.
        /// </summary>
        internal static void Initialize(DalamudPluginInterface pluginInterface)
        {
            PluginConfiguration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            LocalizationService = new LocalizationService();
            WindowingService = new WindowingService();
            ApiModuleService = new RelayApiService();
        }

        /// <summary>
        ///     Disposes of the service class.
        /// </summary>
        internal static void Dispose()
        {
            LocalizationService.Dispose();
            WindowingService.Dispose();
            ApiModuleService.Dispose();
        }
    }
}
