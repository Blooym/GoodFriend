namespace GoodFriend.UI.Screens.Settings;

using System;
using System.IO;
using Dalamud.Interface.ImGuiFileDialog;
using GoodFriend.Base;
using CheapLoc;

sealed public class SettingsPresenter : IDisposable
{
    public bool isVisible = false;

    public void Dispose() { }


#if DEBUG
    public FileDialogManager dialogManager = new FileDialogManager();
    public void OnDirectoryPicked(bool success, string path)
    {
        if (!success) return;
        var directory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(path);
        Loc.ExportLocalizable();
        File.Copy(Path.Combine(path, "GoodFriend_Localizable.json"), Path.Combine(path, "en.json"), true);
        Directory.SetCurrentDirectory(directory);
        PluginService.PluginInterface.UiBuilder.AddNotification("Localization exported successfully.", PStrings.pluginName, Dalamud.Interface.Internal.Notifications.NotificationType.Success);
    }
#endif
}
