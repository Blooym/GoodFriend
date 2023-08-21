using System;
using System.Globalization;
using GoodFriend.Plugin.Base;

namespace GoodFriend.Plugin.Localization
{
    internal sealed class LocalizationService : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        ///     Creates a new resource manager and sets up resources.
        /// </summary>
        public LocalizationService()
        {
            this.SetupLocalization(DalamudInjections.PluginInterface.UiLanguage);
            DalamudInjections.PluginInterface.LanguageChanged += this.SetupLocalization;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.disposedValue)
            {
                return;
            }

            DalamudInjections.PluginInterface.LanguageChanged -= this.SetupLocalization;
            this.disposedValue = true;
        }

        /// <summary>
        ///     Sets up localization for the given language, or uses fallbacks if not found.
        /// </summary>
        /// <param name="language">The language to use.</param>
        private void SetupLocalization(string language)
        {
            try
            {
                Logger.Information($"Setting up localization for {language}");
                Strings.Culture = new CultureInfo(language);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to set language to {language}: {e.Message}");
            }
        }
    }
}
