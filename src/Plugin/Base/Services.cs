using FFXIVClientStructs.FFXIV.Client.System.Framework;
using GoodFriend.Client;
using GoodFriend.Plugin.Configuration;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem;
using GoodFriend.Plugin.UserInterface;

namespace GoodFriend.Plugin.Base
{
    /// <summary>
    ///     Provides access to necessary instances and services.
    /// </summary>
    internal static class Services
    {
        private static LocalizationService LocalizationService { get; set; } = null!;
        public static PluginConfiguration PluginConfiguration { get; private set; } = null!;
        public static WindowingService WindowingService { get; private set; } = null!;
        public static ModuleService ModuleService { get; private set; } = null!;
        public static GoodFriendClient GoodFriendApiClient { get; private set; } = null!;

        /// <summary>
        ///     Initializes the service class.
        /// </summary>
        internal static unsafe void Initialize()
        {
            PluginConfiguration = PluginConfiguration.Load();
            LocalizationService = new LocalizationService();
            WindowingService = new WindowingService();
            GoodFriendApiClient = new GoodFriendClient(new GoodfriendClientOptions()
            {
                BaseAddress = new(PluginConfiguration.ApiConfig.ApiBaseUrl.ToString()),
            }, Framework.Instance()->GameVersion.Base);
            ModuleService = new ModuleService();
        }

        /// <summary>
        ///     Disposes of the service class.
        /// </summary>
        internal static void Dispose()
        {
            LocalizationService.Dispose();
            ModuleService.Dispose();
            WindowingService.Dispose();
            GoodFriendApiClient.Dispose();
        }
    }
}
