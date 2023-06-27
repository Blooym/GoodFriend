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
            SetupLocalization(DalamudInjections.PluginInterface.UiLanguage);
            DalamudInjections.PluginInterface.LanguageChanged += SetupLocalization;
        }

        /// <summary>
        ///     Disposes of the <see cref="LocalizationService" />
        /// </summary>
        public void Dispose()
        {
            if (this.disposedValue)
            {
                return;
            }

            DalamudInjections.PluginInterface.LanguageChanged -= SetupLocalization;
            this.disposedValue = true;
        }

        /// <summary>
        ///     Sets up localization for the given language, or uses fallbacks if not found.
        /// </summary>
        /// <param name="language">The language to use.</param>
        private static void SetupLocalization(string language)
        {
            try
            {
                Logger.Debug($"Setting up localization for {language}");
                Strings.Culture = new CultureInfo(language);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to set language to {language}: {e.Message}");
            }
        }
    }
}
