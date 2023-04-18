using System.Numerics;
using ImGuiNET;

namespace GoodFriend.Plugin.UI.ImGuiBasicComponents
{
    /// <summary>
    ///     Components for positioning elements.
    /// </summary>
    public static class Positioning
    {
        /// <summary>
        ///     Centers the given text on in the middle of the current window.
        /// </summary>
        /// <param name="text"> The text to center. </param>
        /// <param name="colour"> The colour to show the text in, blank for default. </param>
        public static void CenteredText(string text, Vector4? colour = null)
        {
            var size = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - size.X) / 2);
            if (colour != null)
            {
                ImGui.TextColored(colour.Value, text);
            }
            else
            {
                ImGui.Text(text);
            }
        }
    }
}
