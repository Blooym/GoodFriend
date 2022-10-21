namespace GoodFriend.UI.Components
{
    using System.Numerics;
    using ImGuiNET;

    /// <summary> 
    ///     Components and constants for colours.
    /// </summary>
    public static class Colours
    {
        // Generic colours
        public static Vector4 Grey = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

        // Generic "state" colours
        public static Vector4 Success = new Vector4(0.2f, 0.8f, 0.2f, 1.0f);
        public static Vector4 Warning = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
        public static Vector4 Error = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

        // API Status Colours
        public static Vector4 APIConnected = new Vector4(0.2f, 0.8f, 0.2f, 1.0f);
        public static Vector4 APIConnecting = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
        public static Vector4 APIError = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        public static Vector4 APIDisconnected = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        public static Vector4 APIRatelimited = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);

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