using Dalamud.Plugin;
using GoodFriend.Plugin.Base;
using Sirensong;

namespace GoodFriend.Plugin;

public sealed class GoodFriendPlugin : IDalamudPlugin
{
    /// <summary>
    ///     The plugin's main entry point.
    /// </summary>
    /// <param name="pluginInterface"></param>
    public GoodFriendPlugin(IDalamudPluginInterface pluginInterface)
    {
        SirenCore.Initialize(pluginInterface, pluginInterface.Manifest.Name);
        pluginInterface.Create<DalamudInjections>();
        Services.Initialize();

        if (string.IsNullOrEmpty(Constants.Build.ClientKey))
        {
            Logger.Warning("No client key has been set for this build, this is likely to cause issues when connecting to instances that require them.");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Services.Dispose();
        SirenCore.Dispose();
    }
}
