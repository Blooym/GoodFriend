namespace GoodFriend.UI.Components
{
    using System.Numerics;
    using ImGuiNET;

    /// <summary>
    ///     Components for drawing tooltips.
    /// </summary>
    public static class Tooltips
    {
        /// <summary>
        ///     Adds a tooltip on hover to the last item.
        /// </summary>
        /// <param name="text"> The text to show on hover. </param>
        public static void AddTooltipHover(string text)
        {
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(text);
        }

        /// <summary>
        ///    Creates a button with a tooltip on hover. 
        /// <param name="text"> The text to display on the button. </param>
        /// <param name="tooltip"> The text to show on hover. </param>
        /// <param name="size"> The size of the button. </param>
        /// </summary>
        public static bool TooltipButton(string text, string tooltip, Vector2 size = default)
        {
            var result = ImGui.Button(text, size);
            AddTooltipHover(tooltip);
            return result;
        }
    }
}