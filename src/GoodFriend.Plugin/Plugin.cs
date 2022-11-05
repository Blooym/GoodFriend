using Dalamud.IoC;
using Dalamud.Plugin;
using GoodFriend.Base;

namespace GoodFriend
{
    public sealed class GoodFriendPlugin : IDalamudPlugin
    {
        /// <summary> 
        ///     The plugin name, fetched from PStrings.
        /// </summary>
        public string Name => PluginConstants.pluginName;

        /// <summary>
        ///     The plugin's main entry point.
        /// </summary>
        public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            _ = pluginInterface.Create<PluginService>();
            PluginService.Initialize();
        }

        /// <summary>
        ///     Handles disposing of all resources used by the plugin.
        /// </summary>
        public void Dispose()
        {
            PluginService.Dispose();
        }
    }
}