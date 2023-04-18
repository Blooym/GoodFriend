using Dalamud.IoC;
using Dalamud.Plugin;
using GoodFriend.Plugin.Common;
using Sirensong;

namespace GoodFriend.Plugin
{
    public sealed class GoodFriendPlugin : IDalamudPlugin
    {
        /// <summary>
        ///     The plugin name, fetched from PStrings.
        /// </summary>
        public string Name => Constants.PluginName;

        /// <summary>
        ///     The plugin's main entry point.
        /// </summary>
        /// <param name="pluginInterface"></param>
        public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Services>();
            SirenCore.Initialize(pluginInterface, this.Name);
            Services.Initialize();
        }

        /// <summary>
        ///     Handles disposing of all resources used by the plugin.
        /// </summary>
        public void Dispose()
        {
            Services.Dispose();
            SirenCore.Dispose();
        }
    }
}
