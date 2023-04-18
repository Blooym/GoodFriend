using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using GoodFriend.Plugin.Common;
using ToastType = Dalamud.Interface.Internal.Notifications.NotificationType;

namespace GoodFriend.Plugin.Utils
{
    /// <summary>
    ///     All of the notification types.
    /// </summary>
    public enum NotificationType
    {
        Toast = 0,
        Chat = 1,
        Popup = 2,
        QuestPopup = 3,
    }

    /// <summary> Handles the sending of notifications to the client. </summary>
    internal static class Notifications
    {
        /// <summary> Sends a notification to the user using their preferred notification type. </summary>
        /// <param name="message"> The message to send. </param>
        /// <param name="toastType"> The type of toast to send (if sending a toast). </param>
        internal static void ShowPreferred(string message, ToastType? toastType = null)
        {
            var notificationType = Services.Configuration.EventStreamUpdateMessagesConfig.NotificationType;
            Show(message, notificationType, toastType);
        }

        /// <summary> Sends a notification using the given notification type. </summary>
        /// <param name="message"> The message to send. </param>
        /// <param name="type"> The type of notification to send. </param>
        /// <param name="toastType"> The type of toast to send (if sending a toast). </param>
        internal static void Show(string message, NotificationType type, ToastType? toastType = null)
        {
            PluginLog.Debug($"Notifications(Show): Showing {type} notification with the message: {message}");
            switch (type)
            {
                case NotificationType.Toast:
                    ShowToast(message, toastType ?? ToastType.None);
                    break;
                case NotificationType.Chat:
                    ShowChat(message, 35);
                    break;
                case NotificationType.Popup:
                    ShowPopup(message);
                    break;
                case NotificationType.QuestPopup:
                    ShowQuestPopup(message);
                    break;
                default:
                    ShowChat(message, 35);
                    break;
            }
        }

        /// <summary> Sends a toast notification to the user. </summary>
        /// <param name="message"> The message to send. </param>
        /// <param name="type"> The type of toast to send. </param>
        private static void ShowToast(string message, ToastType type) => Services.PluginInterface.UiBuilder.AddNotification(message, Constants.PluginName, type);

        /// <summary> Sends a popup notification to the user. </summary>
        /// <param name="message"> The message to send. </param>
        private static void ShowPopup(string message) => Services.Toast.ShowNormal(message);

        /// <summary> Sends a chat notification to the user. </summary>
        /// <param name="message"> The message to send. </param>
        /// <param name="colour"> The colour to use for the message. </param>
        private static void ShowChat(string message, ushort colour)
        {
            var stringBuilder = new SeStringBuilder()
                .AddUiForeground(colour)
                .AddText(message)
                .AddUiForegroundOff();
            Services.Chat.Print(stringBuilder.BuiltString);
        }

        /// <summary> Sends a quest complete popup notification to the user. </summary>
        /// <param name="message"> The message to send. </param>
        private static void ShowQuestPopup(string message) => Services.Toast.ShowQuest(message, new QuestToastOptions
        {
            Position = QuestToastPosition.Centre,
        });
    }
}
