using Sirensong.Game.Enums;
using Sirensong.Game.Helpers;

namespace GoodFriend.Plugin.Utility
{
    internal static class NotificationUtil
    {
        public static void ShowErrorToast(string message)
        {
            ToastHelper.ShowErrorToast(message);
            SoundEffectHelper.PlaySound(SoundEffect.Se11);
        }
    }
}
