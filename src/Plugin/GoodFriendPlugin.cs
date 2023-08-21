using Dalamud.Plugin;
using GoodFriend.Plugin.Base;
using Sirensong;

namespace GoodFriend.Plugin
{
    public sealed class GoodFriendPlugin : IDalamudPlugin
    {
        /// <summary>
        ///     The plugin name.
        /// </summary>
        public string Name { get; } = Constants.PluginName;

        /// <summary>
        ///     The plugin's main entry point.
        /// </summary>
        /// <param name="pluginInterface"></param>
        public GoodFriendPlugin(DalamudPluginInterface pluginInterface)
        {
            SirenCore.Initialize(pluginInterface, this.Name);
            pluginInterface.Create<DalamudInjections>();
            Services.Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Services.Dispose();
            SirenCore.Dispose();
        }
    }
}
