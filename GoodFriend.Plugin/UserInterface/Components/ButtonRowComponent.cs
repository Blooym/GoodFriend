using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using Sirensong.UserInterface;

namespace GoodFriend.Plugin.UserInterface.Components
{
    internal static class ButtonRowComponent
    {
        /// <summary>
        ///     Draws a row of buttons.
        /// </summary>
        /// <param name="buttons"></param>
        public static void DrawRow(Dictionary<(FontAwesomeIcon, Vector4?, string), Action> buttons)
        {
            if (buttons.Count == 0)
            {
                return;
            }

            foreach (var button in buttons)
            {
                if (ImGuiComponents.IconButton(button.Key.Item1, button.Key.Item2))
                {
                    button.Value();
                }
                SiGui.AddTooltip(button.Key.Item3);
                ImGui.SameLine();
            }
        }
    }
}