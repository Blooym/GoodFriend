using System;
using System.Net;
using System.Net.Http;
using Dalamud.Networking.Http;
using GoodFriend.Client.Http;
using GoodFriend.Client.Http.Requests;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Plugin.Configuration;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem;
using GoodFriend.Plugin.UserInterface;

namespace GoodFriend.Plugin.Base;

/// <summary>
///     Provides access to necessary instances and services.
/// </summary>
internal static class Services
{
    // SSE
    private static readonly TimeSpan SSEReconnectDelayMin = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan SSEReconnectDelayMax = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SSEReconnectDelayIncrement = TimeSpan.FromSeconds(30);
    public static SseClient<PlayerEventStreamUpdate> PlayerEventSseStream { get; private set; } = null!;
    public static SseClient<AnnouncementStreamUpdate> AnnouncementSseStream { get; private set; } = null!;

    private static WindowingService WindowingService { get; set; } = null!;
    private static LocalizationService LocalizationService { get; set; } = null!;
    public static PluginConfiguration PluginConfiguration { get; private set; } = null!;
    public static ModuleService ModuleService { get; private set; } = null!;

    public static HttpClient HttpClient { get; private set; } = null!;
    private static HappyEyeballsCallback HappyEyeballsCallback { get; set; } = null!;

    /// <summary>
    ///     Initializes the service class.
    /// </summary>
    internal static void Initialize()
    {
        HappyEyeballsCallback = new HappyEyeballsCallback();
        PluginConfiguration = PluginConfiguration.Load();
        LocalizationService = new LocalizationService();
        HttpClient = CreateHttpClient(HappyEyeballsCallback);
        PlayerEventSseStream = GetPlayerEventStreamRequest.CreateSSEClient(CreateHttpClient(HappyEyeballsCallback), new()
        {
            ReconnectDelayMin = SSEReconnectDelayMin,
            ReconnectDelayMax = SSEReconnectDelayMax,
            ReconnectDelayIncrement = SSEReconnectDelayIncrement
        });
        AnnouncementSseStream = GetAnnouncementStreamRequest.CreateSSEClient(CreateHttpClient(HappyEyeballsCallback), new()
        {
            ReconnectDelayMin = SSEReconnectDelayMin,
            ReconnectDelayMax = SSEReconnectDelayMax,
            ReconnectDelayIncrement = SSEReconnectDelayIncrement
        });
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
        AnnouncementSseStream.Dispose();
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
        Timeout = TimeSpan.FromSeconds(15),
        DefaultRequestHeaders =
            {
                { "User-Agent", HttpConstants.UserAgent },
                { GlobalRequestData.Headers.ClientKey, Constants.Build.ClientKey ?? "GOODFRIEND_FALLBACK_KEY" }
            },
        DefaultRequestVersion = HttpVersion.Version20,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
    };
}
