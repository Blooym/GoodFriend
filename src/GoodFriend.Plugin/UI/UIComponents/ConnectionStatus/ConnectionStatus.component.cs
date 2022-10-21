namespace GoodFriend.UI.Components
{
    using System.Numerics;
    using GoodFriend.Base;
    using ImGuiNET;

    /// <summary>
    ///     ConnectionStatusComponent shows the current connection status of the API and some flavour text.
    /// </summary>
    public static class ConnectionStatusComponent
    {
        /// <summary>
        ///     The presenter associated with this component.
        /// </summary>
        private static ConnectionStatusPresenter _presenter = new();

        /// <summary>
        ///     Draw the component.
        /// </summary>
        /// <param name="str_id"> The string ID of the component child. </param>
        /// <param name="sizeOverride"> The size of the component. </param>
        public static void Draw(string str_id, Vector2? sizeOverride = null)
        {
            var connectionStatusText = _presenter.GetConnectionStatusText();
            var connectionStatusColour = _presenter.GetConnectionStatusColour();
            var connectionStatusDescription = _presenter.GetConnectionStatusDescription();

            if (ImGui.BeginChild(str_id, sizeOverride ?? new Vector2(0, 80), false, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar))
            {
                // Connection status title
                ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - (ImGui.CalcTextSize(TStrings.EventAPIConnectionStatus).X / 2));
                ImGui.Text(TStrings.EventAPIConnectionStatus);

                // Connection status value
                ImGui.PushStyleColor(ImGuiCol.Button, connectionStatusColour);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, connectionStatusColour / 1.2f);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, connectionStatusColour / 1.4f);
                ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - (ImGui.CalcTextSize(connectionStatusText).X * 2.5f) / 2);
                ImGui.Button(connectionStatusText, new Vector2(ImGui.CalcTextSize(connectionStatusText).X * 2.5f, ImGui.GetFontSize() * 1.3f));
                ImGui.PopStyleColor(3);

                // Connection status description
                Positioning.CenteredText(connectionStatusDescription, Colours.Grey);

                ImGui.EndChild();
            }
        }
    }
}