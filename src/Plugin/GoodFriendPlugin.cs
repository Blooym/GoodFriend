using Dalamud.Plugin;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Utility;
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

            Logger.Information(CryptoUtil.GetSignatureBytes().Length.ToString());
        }

        /// <summary>
        ///     Disposes of the plugin.
        /// </summary>
        public void Dispose() => Services.Dispose();
    }
}
