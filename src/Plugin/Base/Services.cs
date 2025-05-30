using GoodFriend.Networking.SignalR;
using GoodFriend.Plugin.Configuration;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem;
using GoodFriend.Plugin.UserInterface;
using Lumina.Excel;
using Lumina.Excel.Sheets;

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
    public static HubManager HubManager = null!;

    /// <summary>
    ///     Initializes the service class.
    /// </summary>
    internal static void Initialize()
    {
        PluginConfiguration = PluginConfiguration.Load();
        HubManager = new();
        LocalizationService = new LocalizationManager();
        WorldSheet = DalamudInjections.DataManager.GetExcelSheet<World>();
        ModuleService = new ModuleService();
        WindowingService = new WindowingService();
    }

    internal static void Dispose()
    {
        LocalizationService.Dispose();
        WindowingService.Dispose();
        ModuleService.Dispose();
        HubManager.Dispose();
    }
}
