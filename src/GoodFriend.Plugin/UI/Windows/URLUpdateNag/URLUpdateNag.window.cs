using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.Localization;
using GoodFriend.UI.ImGuiComponents;
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
        public URLUpdateNagPresenter presenter;

        /// <summary>
        ///     Disposes of the window and associated resources.
        /// </summary>
        public void Dispose()
        {
            presenter.Dispose();
        }

        /// <summary>
        ///     Instantiate a new settings window.
        /// </summary>
        public URLUpdateNagWindow() : base($"{PluginConstants.pluginName} - URL Update")
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
            return presenter.ShowURLUpdateNag && !presenter.URLUpdateNagDismissed && !URLUpdateNagPresenter.CannotShowNag;
        }

        /// <summary>
        ///     Check if the nag needs to be shown even when the DrawConditions aren't met.
        /// </summary>
        public override void PreOpenCheck()
        {
            IsOpen = presenter.ShowURLUpdateNag;
            if (!presenter.ShowURLUpdateNag && !presenter.URLUpdateNagDismissed) { presenter.HandleURLUpdateNag(); }
        }

        /// <summary>
        ///     Override the OnClose method to make sure the nag is dismissed when the window is closed.
        /// </summary>
        public override void OnClose()
        {
            presenter.URLUpdateNagDismissed = true;
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
            if (_popupTime == null)
            {
                _popupTime = DateTime.Now;
            }

            Colours.TextWrappedColoured(Colours.Warning, URLNagWindow.URLUpdateNagTitle(PluginService.Configuration.APIUrl));
            ImGui.Separator();
            ImGui.TextWrapped(URLNagWindow.URLUpdateNagText(presenter.NewAPIURL, _dismissDelay));
            ImGui.Dummy(new Vector2(0, 5));

            // Options to update or dismiss the nag.
            ImGui.BeginDisabled(!ImGui.IsKeyDown(ImGuiKey.ModShift) && (DateTime.Now - _popupTime.Value).TotalSeconds < _dismissDelay);
            if (ImGui.Button(URLNagWindow.URLUpdateNagButtonUpdate))
            {
#pragma warning disable CS8601 // Checked for null in the presenter
                PluginService.Configuration.APIUrl = presenter.NewAPIURL;
#pragma warning restore CS8601
                PluginService.Configuration.Save();
                PluginLog.Information($"URLUpdateNag(Draw): Accepted suggested URL update, changed API URL to {presenter.NewAPIURL}");
                presenter.URLUpdateNagDismissed = true;
            }
            ImGui.SameLine();

            if (ImGui.Button(URLNagWindow.URLUpdateNagButtonIgnore))
            {
                presenter.URLUpdateNagDismissed = true;
            }
            ImGui.EndDisabled();
        }
    }
}