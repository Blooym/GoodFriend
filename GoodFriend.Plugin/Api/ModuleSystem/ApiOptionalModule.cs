using Dalamud.Interface.Components;
using ImGuiNET;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.Api.ModuleSystem
{
    /// <summary>
    ///     A module that can be enabled or disabled freely at runtime.
    /// </summary>
    internal abstract class ApiOptionalModule : ApiModuleBase
    {
        /// <summary>
        ///     The configuration for this module.
        /// </summary>
        protected abstract ApiOptionalModuleConfig Config { get; }

        /// <summary>
        ///     Whether or not the module is running.
        /// </summary>
        public bool Enabled => this.Config.Enabled;

        /// <inheritdoc />
        protected override void DrawBase()
        {
            SiGui.Heading("Status");
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
            SiGui.AddTooltip("Whether or not the module is enabled.");
            ImGui.SameLine();
            SiGui.Text("Enabled");
            SiGui.AddTooltip("Whether or not the module is enabled.");
            ImGui.Dummy(Spacing.SectionSpacing);

            if (enabled)
            {
                this.DrawModule();
            }
        }
    }

    /// <summary>
    ///     The configuration for <see cref="ApiOptionalModule" />.
    /// </summary>
    internal abstract class ApiOptionalModuleConfig : ApiModuleConfigBase
    {
        /// <summary>
        ///     Whether or not the module is enabled.
        /// </summary>
        public abstract bool Enabled { get; set; }
    }
}