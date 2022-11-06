using System.Numerics;
using GoodFriend.Localization;
using GoodFriend.UI.ImGuiComponents;
using ImGuiNET;

namespace GoodFriend.UI.ImGuiFullComponents.ConnectionStatusComponent
{
    /// <summary>
    ///     ConnectionStatusComponent shows the current connection status of the API and some flavour text.
    /// </summary>
    public static class ConnectionStatusComponent
    {
        /// <summary>
        ///     Draw the component.
        /// </summary>
        /// <param name="strId"> The string ID of the component child. </param>
        /// <param name="sizeOverride"> The size of the component. </param>
        public static void Draw(string strId, Vector2? sizeOverride = null)
        {
            string connectionStatusText = ConnectionStatusPresenter.GetConnectionStatusText();
            Vector4 connectionStatusColour = ConnectionStatusPresenter.GetConnectionStatusColour();
            string connectionStatusDescription = ConnectionStatusPresenter.GetConnectionStatusDescription();

            if (ImGui.BeginChild(strId, sizeOverride ?? new Vector2(0, 80), false, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar))
            {
                // Connection status title
                ImGui.SetCursorPosX((ImGui.GetWindowWidth() / 2) - (ImGui.CalcTextSize(Events.APIConnectionStatus).X / 2));
                ImGui.Text(Events.APIConnectionStatus);

                // Connection status value
                ImGui.PushStyleColor(ImGuiCol.Button, connectionStatusColour);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, connectionStatusColour / 1.2f);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, connectionStatusColour / 1.4f);
                ImGui.SetCursorPosX((ImGui.GetWindowWidth() / 2) - (ImGui.CalcTextSize(connectionStatusText).X * 2.5f / 2));
                _ = ImGui.Button(connectionStatusText, new Vector2(ImGui.CalcTextSize(connectionStatusText).X * 2.5f, ImGui.GetFontSize() * 1.3f));
                ImGui.PopStyleColor(3);

                // Connection status description
                Positioning.CenteredText(connectionStatusDescription, Colours.Grey);

                ImGui.EndChild();
            }
        }
    }
}
