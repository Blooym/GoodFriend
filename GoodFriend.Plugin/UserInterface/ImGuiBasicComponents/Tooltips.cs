using System.Numerics;
using ImGuiNET;

namespace GoodFriend.Plugin.UserInterface.ImGuiBasicComponents
{
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
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(text);
            }
        }

        /// <summary>
        ///    Creates a button with a tooltip on hover.
        /// </summary>
        /// <param name="text"> The text to display on the button. </param>
        /// <param name="tooltip"> The text to show on hover. </param>
        /// <param name="size"> The size of the button. </param>
        public static bool TooltipButton(string text, string tooltip, Vector2 size = default)
        {
            var result = ImGui.Button($"{text}##{tooltip}", size);
            AddTooltipHover(tooltip);
            return result;
        }
    }
}
