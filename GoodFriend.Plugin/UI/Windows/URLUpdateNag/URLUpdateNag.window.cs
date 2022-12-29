using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.Localization;
using GoodFriend.UI.ImGuiBasicComponents;
using ImGuiNET;

namespace GoodFriend.UI.Windows.URLUpdateNag
{
    /// <summary>
    ///     The settings window for the plugin.
    /// </summary>
    public sealed class URLUpdateNagWindow : Window, IDisposable
    {
        /// <summary>
        ///     The presenter associated with the window, handles all logic and data.
        /// </summary>
        internal URLUpdateNagPresenter Presenter;

        /// <summary>
        ///     Disposes of the window and associated resources.
        /// </summary>
        public void Dispose() => this.Presenter.Dispose();

        /// <summary>
        ///     Instantiate a new settings window.
        /// </summary>
        public URLUpdateNagWindow() : base($"{PluginConstants.PluginName} - URL Update")
        {
            this.Size = new Vector2(550, 200);
            this.Flags |= ImGuiWindowFlags.NoDocking;
            this.Flags |= ImGuiWindowFlags.NoResize;
            this.Flags |= ImGuiWindowFlags.NoCollapse;
            this.Flags |= ImGuiWindowFlags.NoSavedSettings;
            this.Flags |= ImGuiWindowFlags.NoNav;
            this.Flags |= ImGuiWindowFlags.NoTitleBar;
            this.Flags |= ImGuiWindowFlags.NoScrollbar;
            this.Flags |= ImGuiWindowFlags.NoFocusOnAppearing;
            this.RespectCloseHotkey = false;

            this.Presenter = new URLUpdateNagPresenter();
        }

        /// <summary>
        ///     Override the DrawConditions to only draw when the nag is needed.
        /// </summary>
        public override bool DrawConditions() => this.Presenter.ShowURLUpdateNag && !this.Presenter.URLUpdateNagDismissed && !URLUpdateNagPresenter.CannotShowNag;

        /// <summary>
        ///     Check if the nag needs to be shown even when the DrawConditions aren't met.
        /// </summary>
        public override void PreOpenCheck() => this.IsOpen = this.Presenter.ShowURLUpdateNag;

        /// <summary>
        ///     Override the OnClose method to make sure the nag is dismissed when the window is closed.
        /// </summary>
        public override void OnClose() => this.Presenter.URLUpdateNagDismissed = true;

        /// <summary>
        ///     The popup datetime of the nag.
        /// </summary>
        private DateTime? popupTime;

        /// <summary>
        ///     How many seconds must pass before the nag can be dismissed.
        /// </summary>
        private const int DismissDelay = 12;

        /// <summary>
        ///     Draw the nag window.
        /// </summary>
        public override void Draw()
        {
            if (this.popupTime == null)
            {
                this.popupTime = DateTime.Now;
            }

            Colours.TextWrappedColoured(Colours.Warning, URLNagWindow.URLUpdateNagTitle(URLUpdateNagPresenter.Configuration.APIUrl));
            ImGui.Separator();
            ImGui.TextWrapped(URLNagWindow.URLUpdateNagText(this.Presenter.NewAPIURL, DismissDelay));
            ImGui.Dummy(new Vector2(0, 5));

            // Options to update or dismiss the nag.
            ImGui.BeginDisabled(!ImGui.IsKeyDown(ImGuiKey.ModShift) && (DateTime.Now - this.popupTime.Value).TotalSeconds < DismissDelay);
            if (ImGui.Button(URLNagWindow.URLUpdateNagButtonUpdate))
            {
#pragma warning disable CS8601 // Checked for null in the presenter
                URLUpdateNagPresenter.Configuration.APIUrl = this.Presenter.NewAPIURL;
#pragma warning restore CS8601
                URLUpdateNagPresenter.Configuration.Save();
                PluginLog.Information($"URLUpdateNag(Draw): Accepted suggested URL update, changed API URL to {this.Presenter.NewAPIURL}");
                this.Presenter.URLUpdateNagDismissed = true;
            }
            ImGui.SameLine();

            if (ImGui.Button(URLNagWindow.URLUpdateNagButtonIgnore))
            {
                this.Presenter.URLUpdateNagDismissed = true;
            }
            ImGui.EndDisabled();
        }
    }
}
