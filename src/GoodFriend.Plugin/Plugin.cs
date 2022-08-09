namespace GoodFriend;

using Dalamud.IoC;
using Dalamud.Plugin;
using GoodFriend.Base;

public sealed unsafe class GoodFriendPlugin : IDalamudPlugin
{
    public string Name => PStrings.pluginName;

    public GoodFriendPlugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PluginService>();
        PluginService.Initialize();

#if !DEBUG
        PluginService.ResourceManager.Update();
#endif

        // uid
        Dalamud.Logging.PluginLog.Debug(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString());
        foreach (var friend in Managers.FriendList.Get())
        {
            Dalamud.Logging.PluginLog.Debug($"{friend->Name} - {Utils.Hashing.HashSHA512(friend->ContentId.ToString())}");
        }
    }


    /// <summary>
    /// Handles disposing of all resources used by the plugin.
    /// </summary>
    public void Dispose() => PluginService.Dispose();
}
