using System;
using GoodFriend.Plugin.Configuration;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem;
using GoodFriend.Plugin.UserInterface;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace GoodFriend.Plugin.Base;

/// <summary>
///     Provides access to necessary instances and services.
/// </summary>
internal static class Services
{
    private static WindowingService WindowingService { get; set; } = null!;
    private static LocalizationManager LocalizationService { get; set; } = null!;
    public static PluginConfiguration PluginConfiguration { get; private set; } = null!;
    public static ModuleService ModuleService { get; private set; } = null!;
    public static ExcelSheet<World> WorldSheet { get; private set; } = null!;
    public static HubConnection PlayerEventHub = null!;

    /// <summary>
    ///     Initializes the service class.
    /// </summary>
    internal static void Initialize()
    {
        PluginConfiguration = PluginConfiguration.Load();
        LocalizationService = new LocalizationManager();
        PlayerEventHub = new HubConnectionBuilder()
            .AddMessagePackProtocol(opt =>
                opt.SerializerOptions
                .WithCompression(MessagePackCompression.Lz4Block)
                .WithResolver(CompositeResolver.Create(
                    StandardResolver.Instance,
                    BuiltinResolver.Instance,
                    AttributeFormatterResolver.Instance,
                    DynamicEnumAsStringResolver.Instance,
                    DynamicGenericResolver.Instance,
                    DynamicUnionResolver.Instance,
                    DynamicObjectResolver.Instance,
                    PrimitiveObjectResolver.Instance,
                    StandardResolver.Instance
                )))
            .WithAutomaticReconnect(new ForeverRetryPolicy())
            .WithUrl(new Uri(PluginConfiguration.ApiConfig.BaseUrl, "/hubs/playerevents"))
            .Build();
        WorldSheet = DalamudInjections.DataManager.GetExcelSheet<World>();
        ModuleService = new ModuleService();
        WindowingService = new WindowingService();
    }

    internal static void Dispose()
    {
        LocalizationService.Dispose();
        WindowingService.Dispose();
        ModuleService.Dispose();
        PlayerEventHub.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    internal sealed class ForeverRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext) => retryContext.PreviousRetryCount switch
        {
            0 => (TimeSpan?)TimeSpan.FromSeconds(10),
            1 => (TimeSpan?)TimeSpan.FromSeconds(30),
            2 => (TimeSpan?)TimeSpan.FromSeconds(60),
            _ => (TimeSpan?)TimeSpan.FromSeconds(120),
        };
    }
}
