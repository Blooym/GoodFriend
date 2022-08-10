namespace GoodFriend.Base;

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.ClientState;
using GoodFriend.Managers;

/// <summary> Provides access to necessary instances and services. </summary>
#pragma warning disable CS8618 // Injection is handled by the Dalamud Plugin Framework here.
sealed internal class PluginService
{
    // Dalamud Services & Instances
    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] internal static ClientState ClientState { get; private set; }
    [PluginService] internal static ChatGui Chat { get; private set; }
    [PluginService] internal static ToastGui Toast { get; private set; }

    // Internal Services & Instances
    internal static WindowManager WindowManager { get; private set; }
    internal static ResourceManager ResourceManager { get; private set; }
    internal static APIClientManager APIClient { get; private set; }
    internal static APINotifier APINotifier { get; private set; }
    internal static Configuration Configuration { get; private set; }

    /// <summary> Initializes the service class. </summary>
    internal static void Initialize()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ResourceManager = new ResourceManager();
        WindowManager = new WindowManager();
        APIClient = new APIClientManager();
        APINotifier = new APINotifier(APIClient);
    }

    /// <summary> Disposes of the service class. </summary>
    internal static void Dispose()
    {
        WindowManager.Dispose();
        ResourceManager.Dispose();
        APIClient.Dispose();
        APINotifier.Dispose();
    }
}
