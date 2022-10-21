namespace GoodFriend.UI.Windows.URLUpdateNag
{
    using System;
    using System.Numerics;
    using GoodFriend.Base;
    using GoodFriend.UI.Components;
    using Dalamud.Interface.Windowing;
    using ImGuiNET;

    /// <summary>
    ///     The settings window for the plugin.
    /// </summary>
    public sealed class URLUpdateNagWindow : Window, IDisposable
    {
        /// <summary>
        ///     The presenter associated with the window, handles all logic and data.
        /// </summary>
        public URLUpdateNagPresenter presenter;

        /// <summary>
        ///     Disposes of the window and associated resources.
        /// </summary>
        public void Dispose() => this.presenter.Dispose();

        /// <summary>
        ///     Instantiate a new settings window.
        /// </summary>
        public URLUpdateNagWindow() : base($"{PStrings.pluginName} - URL Update")
        {
            Size = new Vector2(550, 200);
            Flags |= ImGuiWindowFlags.NoDocking;
            Flags |= ImGuiWindowFlags.NoResize;
            Flags |= ImGuiWindowFlags.NoCollapse;
            Flags |= ImGuiWindowFlags.NoSavedSettings;
            Flags |= ImGuiWindowFlags.NoNav;
            Flags |= ImGuiWindowFlags.NoTitleBar;
            Flags |= ImGuiWindowFlags.NoScrollbar;
            Flags |= ImGuiWindowFlags.NoFocusOnAppearing;
            RespectCloseHotkey = false;

            presenter = new URLUpdateNagPresenter();
        }

        /// <summary>
        ///     Override the DrawConditions to only draw when the nag is needed.
        /// </summary>
        public override bool DrawConditions()
        {
            return presenter.ShowURLUpdateNag && !presenter.URLUpdateNagDismissed && !presenter.CannotShowNag;
        }

        /// <summary>
        ///     Check if the nag needs to be shown even when the DrawConditions aren't met.
        /// </summary>
        public override void PreOpenCheck()
        {
            this.IsOpen = presenter.ShowURLUpdateNag;
            if (!this.presenter.ShowURLUpdateNag) { this.presenter.HandleURLUpdateNag(); }
        }

        /// <summary>
        ///     Override the OnClose method to make sure the nag is dismissed when the window is closed.
        /// </summary>
        public override void OnClose()
        {
            this.presenter.URLUpdateNagDismissed = true;
        }

        /// <summary>
        ///     The popup datetime of the nag.
        /// </summary>
        private DateTime? _popupTime;

        /// <summary>
        ///     How many seconds must pass before the nag can be dismissed.
        /// </summary>
        private const int _dismissDelay = 12;

        /// <summary>
        ///     Draw the nag window.
        /// </summary>
        public override void Draw()
        {
            if (this._popupTime == null) this._popupTime = DateTime.Now;

            Colours.TextWrappedColoured(Colours.Warning, URLNagWindow.URLUpdateNagTitle(PluginService.Configuration.APIUrl));
            ImGui.Separator();
            ImGui.TextWrapped(URLNagWindow.URLUpdateNagText(this.presenter.NewAPIURL ?? new Uri(""), _dismissDelay));
            ImGui.Dummy(new Vector2(0, 5));

            // Options to update or dismiss the nag.
            ImGui.BeginDisabled(!ImGui.IsKeyDown(ImGuiKey.ModShift) && (DateTime.Now - this._popupTime.Value).TotalSeconds < _dismissDelay);
            if (ImGui.Button(URLNagWindow.URLUpdateNagButtonUpdate))
            {
#pragma warning disable CS8601 // Checked for null in the presenter
                PluginService.Configuration.APIUrl = this.presenter.NewAPIURL;
#pragma warning restore CS8601
                PluginService.Configuration.Save();
                this.presenter.URLUpdateNagDismissed = true;
            }
            ImGui.SameLine();

            if (ImGui.Button(URLNagWindow.URLUpdateNagButtonIgnore))
            {
                this.presenter.URLUpdateNagDismissed = true;
            }
            ImGui.EndDisabled();
        }
    }
}