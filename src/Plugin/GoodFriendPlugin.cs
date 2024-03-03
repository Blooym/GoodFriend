using System.Reflection.Metadata;
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

            Logger.Information($"Started GoodFriend {Constants.Build.VersionInformational} ({Constants.Build.GitCommitHash}, {Constants.Build.GitBranch}");
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
}
