namespace GoodFriend.Utils;

using Dalamud.Logging;
using Dalamud.Game.Text.SeStringHandling;
using ToastType = Dalamud.Interface.Internal.Notifications.NotificationType;
using GoodFriend.Base;

/// <summary> Handles the sending of notifications to the client. </summary>
public static class Notifications
{
    /// <summary> Sends a notification to the user using their preferred notification type. </summary>
    /// <param name="message"> The message to send. </param>
    public static void ShowPreferred(string message)
    {
        var notificationType = PluginService.Configuration.NotificationType;
        Show(message, notificationType);
    }


    /// <summary> Sends a notification using the given notification type. </summary>
    public static void Show(string message, NotificationType type, ToastType? notificationType = null)
    {
        PluginLog.Verbose($"Notifications: Showing {type} notification with the message: {message}");
        switch (type)
        {
            case NotificationType.Toast: ShowToast(message, notificationType ?? ToastType.None); break;
            case NotificationType.Chat: ShowChat(message, 35); break;
            case NotificationType.Popup: ShowPopup(message); break;
        }
    }


    /// <summary> Sends a toast notification to the user. </summary>
    /// <param name="message"> The message to send. </param>
    private static void ShowToast(string message, ToastType type) =>
        PluginService.PluginInterface.UiBuilder.AddNotification(message, PStrings.pluginName, type);


    /// <summary> Sends a popup notification to the user. </summary>
    /// <param name="message"> The message to send. </param>
    private static void ShowPopup(string message) =>
        PluginService.Toast.ShowNormal(message);


    /// <summary> Sends a chat notification to the user. </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="colour"> The colour to use for the message. </param>
    private static void ShowChat(string message, ushort colour)
    {
        var stringBuilder = new SeStringBuilder();
        stringBuilder.AddUiForeground(colour);
        stringBuilder.AddText(message);
        stringBuilder.AddUiForegroundOff();
        PluginService.Chat.Print(stringBuilder.BuiltString);
    }

}