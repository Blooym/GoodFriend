namespace GoodFriend.UI.Windows.Main
{
    using System;
    using System.IO;
    using GoodFriend.Base;
    using CheapLoc;
    using Dalamud.Interface.ImGuiFileDialog;

    public sealed class MainPresenter : IDisposable
    {
        public void Dispose() { }

        /// <summary>
        ///     Stores which dropdown menu is currently open.
        /// </summary>
        public VisibleDropdown visibleDropdown { get; set; } = VisibleDropdown.None;

        /// <summary> 
        ///     Toggles the selected dropdown. If a dropdown of the same type is already open, the type is set to <see cref="VisibleDropdown.None"/>.
        /// </summary>
        public void ToggleVisibleDropdown(VisibleDropdown dropdown)
        {
            if (dropdown == visibleDropdown)
            {
                visibleDropdown = VisibleDropdown.None;
            }
            else
            {
                visibleDropdown = dropdown;
            }
        }

#if DEBUG
        /// <summary>
        ///     The dialog manager for the settings window.
        /// </summary>
        public FileDialogManager dialogManager = new FileDialogManager();

        /// <summary>
        ///     The callback for when the user selects an export directory. 
        /// </summary>
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

        public enum VisibleDropdown
        {
            None,
            Donate,
            Logs,
            Settings,
        }
    }
}