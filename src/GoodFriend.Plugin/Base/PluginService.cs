namespace GoodFriend.Base;

using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using Dalamud.Plugin;

/// <summary>
///     Service is an internal class that provides internal access to necessary services.
/// </summary>
#pragma warning disable CS8618 // Injection is handled by the Dalamud Plugin Framework.
internal class Service
{
    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] internal static ClientState ClientState { get; private set; }
    [PluginService] internal static ChatGui Chat { get; private set; }
    [PluginService] internal static ToastGui Toast { get; private set; }
    internal static Configuration Configuration { get; private set; }

    internal static void Initialize(Configuration configuration) { Configuration = configuration; configuration.Save(); }
}
