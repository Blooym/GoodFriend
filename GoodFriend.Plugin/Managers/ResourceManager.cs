using System;
using System.IO;
using System.Reflection;
using CheapLoc;
using Dalamud.Logging;
using GoodFriend.Base;

namespace GoodFriend.Managers
{
    /// <summary>
    ///     Sets up and manages the plugin's resources and localization.
    /// </summary>
    internal sealed class ResourceManager : IDisposable
    {
        /// <summary>
        ///     Initializes the ResourceManager and associated resources.
        /// </summary>
        internal ResourceManager()
        {
            PluginLog.Debug("ResourceManager(ResourceManager): Initializing...");

            this.Setup(PluginService.PluginInterface.UiLanguage);
            PluginService.PluginInterface.LanguageChanged += this.Setup;

            PluginLog.Debug("ResourceManager(ResourceManager): Initialization complete.");
        }

        /// <summary>
        ///      Disposes of the ResourceManager and associated resources.
        /// </summary>
        public void Dispose()
        {
            PluginService.PluginInterface.LanguageChanged -= this.Setup;

            PluginLog.Debug("ResourceManager(Dispose): Successfully disposed.");
        }

        /// <summary>
        ///     Sets up the plugin's resources.
        /// </summary>
        /// <param name="language">The new language 2-letter code.</param>
        private void Setup(string language)
        {
            try
            {
                using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream($"GoodFriend.Resources.Localization.{language}.json") ?? throw new FileNotFoundException($"Could not find resource file for language {language}.");
                using var reader = new StreamReader(resource);
                Loc.Setup(reader.ReadToEnd());
                PluginLog.Information($"ResourceManager(Setup): Resource file for language {language} loaded successfully.");
            }
            catch (Exception e)
            {
                PluginLog.Information($"ResourceManager(Setup): Falling back to English resource file. ({e.Message})");
                Loc.SetupWithFallbacks();
            }
        }
    }
}
