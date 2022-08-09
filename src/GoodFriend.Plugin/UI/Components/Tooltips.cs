namespace GoodFriend.UI.Components;

using ImGuiNET;

static class Tooltips
{
    /// <summary> Adds a tooltip on hover to the last item. </summary>
    /// <param name="text"> The text to show on hover. </param>
    public static void AddTooltip(string text)
    {
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(text);
    }


    /// <summary> Draws a ? that when hovering will show the given text as a tooltip on the same line. </summary>
    /// <param name="text"> The text to show as a tooltip. </param>
    public static void Questionmark(string text)
    {
        ImGui.SameLine();
        ImGui.TextColored(Colours.Grey, "[?]");
        Components.Tooltips.AddTooltip(text);
    }


    /// <summary> Draws a ! that when hovering will show the given text as a tooltip on the same line. </summary>
    /// <param name="text"> The text to show as a tooltip. </param>
    public static void Warning(string text)
    {
        ImGui.SameLine();
        ImGui.TextColored(Colours.Warning, "[ALERT]");
        Components.Tooltips.AddTooltip(text);
    }
}