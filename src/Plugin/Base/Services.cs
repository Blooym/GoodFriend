using System.Net;
using System.Net.Http;
using System.Timers;
using Dalamud.Networking.Http;
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
        private static WindowingService WindowingService { get; set; } = null!;
        private static HappyEyeballsCallback HappyEyeballsCallback { get; set; } = null!;

        private static LocalizationService LocalizationService { get; set; } = null!;
        public static PluginConfiguration PluginConfiguration { get; private set; } = null!;
        public static ModuleService ModuleService { get; private set; } = null!;
        public static SseClient<PlayerEventStreamUpdate> PlayerEventSseStream { get; private set; } = null!;
        public static HttpClient HttpClient { get; private set; } = null!;

        /// <summary>
        ///     Initializes the service class.
        /// </summary>
        internal static void Initialize()
        {
            HappyEyeballsCallback = new HappyEyeballsCallback();
            PluginConfiguration = PluginConfiguration.Load();
            LocalizationService = new LocalizationService();
            HttpClient = CreateHttpClient(HappyEyeballsCallback);
            PlayerEventSseStream = GetPlayerEventStreamRequest.CreateSSEClient(CreateHttpClient(HappyEyeballsCallback), new Timer(60000));
            ModuleService = new ModuleService();
            WindowingService = new WindowingService();
        }

        /// <inheritdoc />
        internal static void Dispose()
        {
            LocalizationService.Dispose();
            WindowingService.Dispose();
            ModuleService.Dispose();
            HttpClient.Dispose();
            PlayerEventSseStream.Dispose();
            HappyEyeballsCallback.Dispose();
        }

        /// <summary>
        ///     Creates a new <see cref="HttpClient" /> with the given <see cref="HappyEyeballsCallback" />.
        /// </summary>
        /// <param name="happyEyeballsCallback"></param>
        /// <returns></returns>
        private static HttpClient CreateHttpClient(HappyEyeballsCallback happyEyeballsCallback) => new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            ConnectCallback = happyEyeballsCallback.ConnectCallback,
        })
        {
            BaseAddress = PluginConfiguration.ApiConfig.BaseUrl,
            Timeout = System.TimeSpan.FromSeconds(20),
            DefaultRequestHeaders =
                {
                    { "User-Agent", HttpConstants.UserAgent },
                },
            DefaultRequestVersion = HttpVersion.Version20,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
        };
    }
}
