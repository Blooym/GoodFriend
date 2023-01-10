using System;
using ImGuiNET;

namespace GoodFriend.UI.ImGuiFullComponents.TextInput
{
    /// <summary>
    ///     Text input component.
    /// </summary>
    internal sealed class TextInput
    {
        /// <summary>
        ///     Draws a text input with a label and a save action.
        /// </summary>
        /// <param name="label">The ImGuI label.</param>
        /// <param name="value">The reference to the value.</param>
        /// <param name="saveAction">The action to be executed when the input is deactivated.</param>
        /// <param name="maxLength">The maximum length of the input.</param>
        /// <param name="placeholder">The placeholder text.</param>
        /// <returns></returns>
        internal static bool Draw(string label, ref string value, Action saveAction, uint maxLength = 2048, string placeholder = "")
        {
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() * 0.5f);
            var input = ImGui.InputTextWithHint(label, placeholder, ref value, maxLength);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                saveAction();
            }

            return input;
        }
    }
}
