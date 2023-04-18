using System.Numerics;
using ImGuiNET;

namespace GoodFriend.Plugin.UI.ImGuiBasicComponents
{
    /// <summary>
    ///     Components and constants for colours.
    /// </summary>
    public static class Colours
    {
        public static readonly Vector4 Grey = new(0.5f, 0.5f, 0.5f, 1.0f);

        public static readonly Vector4 Success = new(0.2f, 0.8f, 0.2f, 1.0f);
        public static readonly Vector4 Warning = new(1.0f, 0.5f, 0.0f, 1.0f);
        public static readonly Vector4 Error = new(1.0f, 0.0f, 0.0f, 1.0f);

        public static readonly Vector4 APIConnected = new(0.2f, 0.8f, 0.2f, 1.0f);
        public static readonly Vector4 APIConnecting = new(1.0f, 0.5f, 0.0f, 1.0f);
        public static readonly Vector4 APIError = new(1.0f, 0.0f, 0.0f, 1.0f);
        public static readonly Vector4 APIDisconnected = new(0.5f, 0.5f, 0.5f, 1.0f);

        /// <summary>
        ///     Creates a ImgGui.TextWrapped() with the given text and colour.
        /// </summary>
        /// <param name="colour"> The colour to show the text in. </param>
        /// <param name="text"> The text to show. </param>
        public static void TextWrappedColoured(Vector4 colour, string text)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, colour);
            ImGui.TextWrapped(text);
            ImGui.PopStyleColor();
        }
    }
}
