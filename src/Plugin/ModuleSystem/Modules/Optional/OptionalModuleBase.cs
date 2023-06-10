using Dalamud.Interface.Components;
using GoodFriend.Plugin.Localization;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules.Optional
{
    /// <summary>
    ///     A module that can be enabled or disabled freely at runtime.
    /// </summary>
    internal abstract class OptionalModuleBase : ModuleBase
    {
        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        protected abstract OptionalModuleConfigBase Config { get; }

        /// <summary>
        ///     Whether or not the module is running.
        /// </summary>
        public bool Enabled => this.Config.Enabled;

        /// <inheritdoc />
        protected override void DrawBase()
        {
            SiGui.Heading(Strings.Modules_OptionalModuleBase_Enabled);
            var enabled = this.Config.Enabled;

            if (ImGuiComponents.ToggleButton($"Enabled##{this.GetType().FullName}", ref enabled))
            {
                this.Config.Enabled = enabled;
                this.Config.Save();
                if (enabled)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            }
            SiGui.AddTooltip(Strings.Modules_OptionalModuleBase_EnabledSwitch_Tooltip);
            ImGui.SameLine();
            SiGui.Text(Strings.Modules_OptionalModuleBase_EnabledSwitch);
            SiGui.AddTooltip(Strings.Modules_OptionalModuleBase_EnabledSwitch_Tooltip);
            ImGui.Dummy(Spacing.SectionSpacing);

            if (enabled)
            {
                this.DrawModule();
            }
        }
    }

    /// <summary>
    ///     The configuration for <see cref="OptionalModuleBase" />.
    /// </summary>
    internal abstract class OptionalModuleConfigBase : ModuleConfigBase
    {
        /// <summary>
        ///     Whether or not the module is enabled.
        /// </summary>
        public abstract bool Enabled { get; set; }
    }
}
