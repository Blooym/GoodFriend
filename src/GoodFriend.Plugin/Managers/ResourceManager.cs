namespace GoodFriend.Managers
{
    using System;
    using System.Threading;
    using System.IO;
    using System.Net.Http;
    using System.IO.Compression;
    using GoodFriend.Base;
    using Dalamud.Logging;
    using CheapLoc;

    /// <summary>
    ///     Sets up and manages the plugin's resources and localization.
    /// </summary>
    internal sealed class ResourceManager : IDisposable
    {
        internal event ResourceUpdateDelegate? ResourcesUpdated;
        internal delegate void ResourceUpdateDelegate();

        /// <summary>
        ///     Initializes the ResourceManager and associated resources.
        /// </summary>
        internal ResourceManager()
        {
            PluginLog.Verbose("ResourceManager(ResourceManager): Initializing...");

            this.Setup(PluginService.PluginInterface.UiLanguage);
            PluginService.PluginInterface.LanguageChanged += this.Setup;
            ResourcesUpdated += this.OnResourceUpdate;

            PluginLog.Verbose("ResourceManager(ResourceManager): Initialization complete.");
        }

        /// <summary>
        //      Disposes of the ResourceManager and associated resources.
        /// </summary>
        public void Dispose()
        {
            PluginLog.Verbose("ResourceManager(Dispose): Disposing...");

            PluginService.PluginInterface.LanguageChanged -= Setup;
            ResourcesUpdated -= OnResourceUpdate;

            PluginLog.Verbose("ResourceManager(Dispose): Successfully disposed.");
        }

        /// <summary> 
        ///     Downloads the repository from GitHub and extracts the resource data.
        /// </summary>
        internal void Update()
        {
            var repoName = PStrings.pluginName.Replace(" ", "");
            var zipFilePath = Path.Combine(Path.GetTempPath(), $"{repoName}.zip");
            var zipExtractPath = Path.Combine(Path.GetTempPath(), $"{repoName}-{PStrings.repoBranch}", $"{PStrings.repoResourcesDir}");
            var pluginExtractPath = Path.Combine(PStrings.pluginResourcesDir);

            // NOTE: This is only GitHub compatible, changes will need to be made here for other providers as necessary.
            new Thread(() =>
            {
                try
                {
                    PluginLog.Verbose($"ResourceManager(Update): Opening new thread to handle resource file download and extraction.");

                    // Download the files from the repository and extract them into the temp directory.
                    using var client = new HttpClient();
                    client.GetAsync($"{PStrings.repoUrl}archive/refs/heads/{PStrings.repoBranch}.zip").ContinueWith((task) =>
                    {
                        using var stream = task.Result.Content.ReadAsStreamAsync().Result;
                        using var fileStream = File.Create(zipFilePath);
                        stream.CopyTo(fileStream);
                    }).Wait();
                    PluginLog.Verbose($"ResourceManager(Update): Downloaded resource files to: {zipFilePath}");

                    // Extract the zip file and copy the resources.
                    ZipFile.ExtractToDirectory(zipFilePath, Path.GetTempPath(), true);
                    foreach (string dirPath in Directory.GetDirectories(zipExtractPath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(zipExtractPath, pluginExtractPath));
                        PluginLog.Verbose($"ResourceManager(Update): Created directory: {dirPath.Replace(zipExtractPath, pluginExtractPath)}");
                    }

                    foreach (string newPath in Directory.GetFiles(zipExtractPath, "*.*", SearchOption.AllDirectories))
                    {
                        PluginLog.Verbose($"ResourceManager(Update): Copying file from: {newPath} to: {newPath.Replace(zipExtractPath, pluginExtractPath)}");
                        File.Copy(newPath, newPath.Replace(zipExtractPath, pluginExtractPath), true);
                    }

                    // Cleanup temporary files.
                    File.Delete(zipFilePath);
                    Directory.Delete($"{Path.GetTempPath()}{repoName}-{PStrings.repoBranch}", true);
                    PluginLog.Verbose($"ResourceManager(Update): Deleted temporary files.");

                    // Broadcast an event indicating that the resources have been updated.
                    ResourcesUpdated?.Invoke();
                }
                catch (Exception e) { PluginLog.Error($"ResourceManager(Update): Error updating resource files: {e.Message}"); }
            }).Start();
        }

        /// <summary>
        ///     Handles the OnResourceUpdate event.
        /// </summary>
        private void OnResourceUpdate()
        {
            PluginLog.Verbose($"ResourceManager(OnResourceUpdate): Resources updated.");
            this.Setup(PluginService.PluginInterface.UiLanguage);
        }

        /// <summary>
        ///     Sets up the plugin's resources.
        /// </summary>
        private void Setup(string language)
        {
            PluginLog.Log($"ResourceManager(Setup): Setting up resources for language {language}...");

            try { Loc.Setup(File.ReadAllText($"{PStrings.pluginlocalizationDir}{language}.json")); }
            catch { Loc.SetupWithFallbacks(); }

            PluginLog.Verbose("ResourceManager(Setup): Resources setup.");
        }
    }
}