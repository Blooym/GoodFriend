using System;
using System.Collections.Generic;
using System.IO;
using CheapLoc;
using Dalamud.Interface.ImGuiFileDialog;
using GoodFriend.Base;
using GoodFriend.Managers;
using GoodFriend.Managers.FriendList;
using GoodFriend.Types;

namespace GoodFriend.UI.Windows.Main
{
    public sealed class MainPresenter : IDisposable
    {
        public void Dispose() { }

        /// <summary>
        ///     Stores which dropdown menu is currently open.
        /// </summary>
        public VisibleDropdown CurrentVisibleDropdown { get; set; } = VisibleDropdown.None;

        /// <summary>
        ///     Boolean value indicating if the user needs to restart to apply changes.
        /// </summary>
        public bool RestartToApply { get; set; }

        /// <summary>
        ///     The event log instance to use in the UI.
        /// </summary>
        internal static EventLogManager EventLog => PluginService.EventLogManager;

        /// <summary>
        ///     Where to get metadata from.
        /// </summary>
        public static APIClient.MetadataPayload? Metadata => PluginService.APIClientManager.MetadataCache;

        /// <summary>
        ///     Toggles the selected dropdown. If a dropdown of the same type is already open, the type is set to <see cref="VisibleDropdown.None"/>.
        /// </summary>
        public void ToggleVisibleDropdown(VisibleDropdown dropdown) => this.CurrentVisibleDropdown = dropdown == this.CurrentVisibleDropdown ? VisibleDropdown.None : dropdown;

        /// <summary>
        ///     Fetches the users friendslist from the game.
        /// </summary>
        public unsafe List<FriendListEntry> GetFriendList()
        {
            List<FriendListEntry> friends = new();
            foreach (FriendListEntry* friend in FriendList.Get())
            {
                friends.Add(*friend);
            }
            return friends;
        }

#if DEBUG
        /// <summary>
        ///     The dialog manager for the settings window.
        /// </summary>
        internal FileDialogManager DialogManager = new();

        /// <summary>
        ///     The callback for when the user selects an export directory.
        /// </summary>
        public static void OnDirectoryPicked(bool cancelled, string path)
        {
            if (!cancelled)
            {
                return;
            }

            var directory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(path);
            Loc.ExportLocalizable();
            File.Copy(Path.Combine(path, "GoodFriend_Localizable.json"), Path.Combine(path, "en.json"), true);
            Directory.SetCurrentDirectory(directory);
            PluginService.PluginInterface.UiBuilder.AddNotification("Localization exported successfully.", PluginConstants.PluginName, Dalamud.Interface.Internal.Notifications.NotificationType.Success);
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
