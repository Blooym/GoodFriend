namespace GoodFriend;

using Dalamud.IoC;
using Dalamud.Plugin;
using GoodFriend.Base;

public sealed unsafe class GoodFriendPlugin : IDalamudPlugin
{
    /// <summary> 
    ///     The plugin name, fetched from PStrings.
    /// </summary>
    public string Name => "ignore this change";
    
    /// <summary>
    ///     The plugin's main entry point.
    /// </summary>
    public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PluginService>();
        PluginService.Initialize();

        // #if !DEBUG
        PluginService.ResourceManager.Update();
        // #endif
    }


    /// <summary>
    ///     Handles disposing of all resources used by the plugin.
    /// </summary>
    public void Dispose() => PluginService.Dispose();
}
