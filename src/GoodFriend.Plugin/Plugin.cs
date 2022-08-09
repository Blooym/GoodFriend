namespace GoodFriend;

using Dalamud.IoC;
using Dalamud.Plugin;
using GoodFriend.Base;

public sealed class GoodFriendPlugin : IDalamudPlugin
{
    public string Name => PStrings.pluginName;

    public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PluginService>();
        PluginService.Initialize();

#if !DEBUG
        PluginService.ResourceManager.Update();
#endif

        Dalamud.Logging.PluginLog.Warning(Utils.Hashing.HashSHA512("1"));
    }


    /// <summary>
    /// Handles disposing of all resources used by the plugin.
    /// </summary>
    public void Dispose() => PluginService.Dispose();
}
