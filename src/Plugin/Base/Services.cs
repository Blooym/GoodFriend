using System.Net.Http;
using System.Timers;
using GoodFriend.Client.Http;
using GoodFriend.Client.Http.Requests;
using GoodFriend.Client.Http.Responses;
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
        public static SSEClient<PlayerEventStreamUpdate> PlayerEventSSEStream { get; private set; } = null!;
        public static HttpClient HttpClient { get; private set; } = null!;

        /// <summary>
        ///     Initializes the service class.
        /// </summary>
        internal static unsafe void Initialize()
        {
            PluginConfiguration = PluginConfiguration.Load();
            LocalizationService = new LocalizationService();
            PlayerEventSSEStream = GetPlayerEventStreamRequest.CreateSSEClient(HttpClientUtil.CreateHttpClient(PluginConfiguration.ApiConfig.BaseUrl), new Timer(60000));
            HttpClient = HttpClientUtil.CreateHttpClient(PluginConfiguration.ApiConfig.BaseUrl);
            ModuleService = new ModuleService();
            WindowingService = new WindowingService();
        }

        /// <summary>
        ///     Disposes of the service class.
        /// </summary>
        internal static void Dispose()
        {
            LocalizationService.Dispose();
            WindowingService.Dispose();
            ModuleService.Dispose();
            HttpClient.Dispose();
            PlayerEventSSEStream.Dispose();
        }
    }
}
