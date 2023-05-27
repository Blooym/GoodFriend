using System;
using System.Collections.Generic;
using System.IO;
using CheapLoc;
using Dalamud.Interface.ImGuiFileDialog;
using GoodFriend.Base;
using GoodFriend.Managers;
using GoodFriend.Managers.FriendList;
using GoodFriend.Types;
using XivCommon.Functions.FriendList;

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
        ///     The filter to apply to the event log.
        /// </summary>
        internal EventLogManager.EventLogType EventLogFilter { get; set; } = EventLogManager.EventLogType.Info;

        /// <summary>
        ///     Boolean value indicating if the user needs to restart to apply changes.
        /// </summary>
        public bool RestartToApply { get; set; }

        /// <summary>
        ///     The event log instance to use in the UI.
        /// </summary>
        internal static EventLogManager EventLog => PluginService.EventLogManager;

        /// <summary>
        ///     The metadata cache instance to use in the UI.
        /// </summary>
        public static APIClient.MetadataPayload? Metadata => PluginService.APIClientManager.MetadataCache;

        /// <summary>
        ///     Toggles the selected dropdown. If a dropdown of the same type is already open, the type is set to <see cref="VisibleDropdown.None"/>.
        /// </summary>
        /// <param name="dropdown"></param>
        public void ToggleVisibleDropdown(VisibleDropdown dropdown) => this.CurrentVisibleDropdown = dropdown == this.CurrentVisibleDropdown ? VisibleDropdown.None : dropdown;

        /// <summary>
        ///     The currently visible dropdown.
        /// </summary>
        public enum VisibleDropdown
        {
            None = 0,
            Donate = 1,
            Logs = 2,
            Settings = 3,
        }

#if DEBUG
        /// <summary>
        ///     Fetches the users friendslist from the game.
        /// </summary>
        public static unsafe List<FriendListEntry> GetFriendList()
        {
            List<FriendListEntry> friends = new();
            foreach (var friend in FriendListCache.Get())
            {
                friends.Add(friend);
            }
            return friends;
        }

        /// <summary>
        ///     The dialog manager for the settings window.
        /// </summary>
        internal FileDialogManager DialogManager = new();

        /// <summary>
        ///     The callback for when the user selects an export directory.
        /// </summary>
        /// <param name="cancelled">Whether the user cancelled the dialog.</param>
        /// <param name="path">The path the user selected.</param>
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
    }
}
