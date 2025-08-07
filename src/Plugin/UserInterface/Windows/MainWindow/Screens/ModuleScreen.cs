using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.ModuleSystem.Modules;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.UserInterface.Windows.MainWindow.Screens;

// TODO: cleanup the gui code here.
internal static class ModuleScreen
{
    /// <summary>
    ///     The current module.
    /// </summary>
    private static BaseModule? CurrentModule { get; set; } = Services.ModuleService.GetModule<LoginStateModule>();

    /// <summary>
    ///     Draws the list panel of the main window.
    /// </summary>
    public static void DrawModuleList()
    {
        ModuleTag? lastTag = null;
        foreach (var module in Services.ModuleService.GetModules().OrderBy(x => x.Tag).ThenBy(x => x.Name).ThenByDescending(x => x.DisplayWeight))
        {
            if (lastTag != module.Tag)
            {
                if (lastTag is not null)
                {
                    ImGui.Dummy(Spacing.SidebarSectionSpacing);
                }
                lastTag = module.Tag;
                SiGui.Heading(module.Tag.ToString());
            }
            else
            {
                ImGui.Dummy(new(0, 1.5f));
            }

            if (ImGui.Selectable(module.Name, CurrentModule == module))
            {
                CurrentModule = module;
            }
        }
    }

    /// <summary>
    ///     Draw the detail panel for the selected module.
    /// </summary>
    public static void DrawModuleDetails()
    {
        if (CurrentModule is null)
        {
            ImGui.SetCursorPos(new((ImGui.GetContentRegionAvail().X / 2) - (ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_SelectToView).X / 2), (ImGui.GetContentRegionAvail().Y / 2) - (ImGui.CalcTextSize(Strings.UI_MainWindow_ModuleScreen_SelectToView).Y / 2)));
            SiGui.TextDisabledWrapped(Strings.UI_MainWindow_ModuleScreen_SelectToView);
            return;
        }

        SiGui.TextDisabledWrapped(CurrentModule.Name);
        ImGui.Dummy(Spacing.HeaderSpacing);

        if (!string.IsNullOrEmpty(CurrentModule.Description))
        {
            SiGui.TextWrapped(CurrentModule.Description);
            ImGui.Dummy(Spacing.SectionSpacing);
        }
        using (var child = ImRaii.Child("PluginSettingsListChildScrolling"))
        {
            if (child.Success)
            {
                CurrentModule.Draw();
            }
        }
    }
}
