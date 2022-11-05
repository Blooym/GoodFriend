using GoodFriend.Localization;

namespace GoodFriend.Enums
{
    /// <summary>
    ///     All of the notification types.
    /// </summary>
    public enum NotificationType
    {
        [LocalizableName("NotificationType.Toast", "Toast")]
        [LocalizableDescription("NotificationType.Toast.Description", "A toast notification that appears in the corner of the screen.")]
        Toast,

        [LocalizableName("NotificationType.Chat", "Chat")]
        [LocalizableDescription("NotificationType.Chat.Description", "A chat notification that appears in the chat window.")]
        Chat,

        [LocalizableName("NotificationType.Popup", "Popup")]
        [LocalizableDescription("NotificationType.Popup.Description", "A popup notification that appears in the center of the screen.")]
        Popup
    }
}