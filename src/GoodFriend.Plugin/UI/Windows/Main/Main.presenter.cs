namespace GoodFriend.UI.Windows.Main
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using GoodFriend.Base;
    using GoodFriend.Managers;
    using GoodFriend.Types;
    using Dalamud.Interface.ImGuiFileDialog;
    using CheapLoc;

    public sealed class MainPresenter : IDisposable
    {
        public void Dispose() { }

        /// <summary>
        ///     Stores which dropdown menu is currently open.
        /// </summary>
        public VisibleDropdown visibleDropdown { get; set; } = VisibleDropdown.None;

        /// <summary>
        ///     Boolean value indicating if the user needs to restart to apply changes.
        /// </summary>
        public bool restartToApply { get; set; } = false;

        /// <summary>
        ///     The event log instance to use in the UI.
        /// </summary>
        internal EventLogManager EventLog { get; } = PluginService.EventLogManager;

        /// <summary>
        ///     Where to get metadata from.
        /// </summary>
        public APIClient.MetadataPayload? Metadata => PluginService.APIClientManager.GetMetadata();

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

        /// <summary>
        ///     Fetches the users friendslist from the game.
        /// </summary>
        public unsafe List<FriendListEntry> GetFriendList()
        {
            var friends = new List<FriendListEntry>();
            foreach (var friend in FriendList.Get())
            {
                friends.Add(*friend);
            }
            return friends;
        }

#if DEBUG
        /// <summary>
        ///     The dialog manager for the settings window.
        /// </summary>
        public FileDialogManager dialogManager = new FileDialogManager();

        /// <summary>
        ///     The callback for when the user selects an export directory. 
        /// </summary>
        public void OnDirectoryPicked(bool cancelled, string path)
        {
            if (!cancelled) return;

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