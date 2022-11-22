using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using GoodFriend.Base;
using GoodFriend.Utils;

namespace GoodFriend
{
    public sealed class GoodFriendPlugin : IDalamudPlugin
    {
        /// <summary>
        ///     The plugin name, fetched from PStrings.
        /// </summary>
        public string Name => PluginConstants.PluginName;

        /// <summary>
        ///     The plugin's main entry point.
        /// </summary>
        /// <param name="pluginInterface"></param>
        public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<PluginService>();
            PluginService.Initialize();

            if (!Common.IsOfficialSource && !Common.IsDevBuild)
            {
                PluginLog.Warning("This plugin has been downloaded from a 3rd-party source and may be unsafe. Consider using the official source instead. NO SUPPORT WILL BE PROVIDED FOR 3RD-PARTY SOURCES OR MODIFIED BUILDS.");
            }
        }

        /// <summary>
        ///     Handles disposing of all resources used by the plugin.
        /// </summary>
        public void Dispose() => PluginService.Dispose();
    }
}
