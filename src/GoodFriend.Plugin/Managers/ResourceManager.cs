namespace GoodFriend.Managers;

using System;
using System.Threading;
using System.IO;
using System.Net;
using System.IO.Compression;
using CheapLoc;
using Dalamud.Logging;
using GoodFriend.Base;


/// <summary> Sets up and manages the plugin's resources and localization. </summary>
sealed public class ResourceManager : IDisposable
{
    public event ResourceUpdateDelegate? ResourcesUpdated;
    public delegate void ResourceUpdateDelegate();


    /// <summary> Initializes the ResourceManager and associated resources. </summary>
    public ResourceManager()
    {
        PluginLog.Debug("ResourceManager: Initializing...");

        Setup(PluginService.PluginInterface.UiLanguage);
        PluginService.PluginInterface.LanguageChanged += Setup;
        ResourcesUpdated += OnResourceUpdate;

        PluginLog.Debug("ResourceManager: Initialization complete.");
    }


    /// <summary> Disposes of the ResourceManager and associated resources. </summary>
    public void Dispose()
    {
        PluginLog.Debug("ResourceManager: Disposing...");

        PluginService.PluginInterface.LanguageChanged -= Setup;
        ResourcesUpdated -= OnResourceUpdate;

        PluginLog.Debug("ResourceManager: Successfully disposed.");
    }


    /// <summary> Downloads the repository from GitHub and extracts the resource data. </summary>
    public void Update()
    {
        new Thread(() =>
        {
            try
            {
                PluginLog.Debug($"ResourceManager: Opening new thread to handle resource download.");

                // Create a new WebClient to download the data and some paths for installation.
                var webClient = new WebClient();
                var zipFile = Path.Combine(Path.GetTempPath(), "GoodFriend_Source.zip");
                var sourcePath = Path.Combine(Path.GetTempPath(), "GoodFriend-main", "src", "GoodFriend.Plugin", "Resources");
                var targetPath = Path.Combine(PStrings.resourcePath);

                // Download the file into the system temp directory to make sure it can be cleaned up by the OS incase of a crash.
                webClient.DownloadFile($"{PStrings.pluginRepository}archive/refs/heads/main.zip", zipFile);

                // Extract the zip file into the system temp directory and delete the zip file.
                ZipFile.ExtractToDirectory(zipFile, Path.GetTempPath(), true);

                // Create directories & copy files.
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);

                // Delete the temporary files.
                File.Delete(zipFile);
                Directory.Delete($"{Path.GetTempPath()}GoodFriend-main", true);


                // Broadcast an event indicating that the resources have been updated & refresh the UI.
                ResourcesUpdated?.Invoke();
            }

            catch (Exception e)
            {
                PluginLog.Error($"ResourceManager: Error updating resource files: {e.Message}");
            }

        }).Start();
    }


    /// <summary> Handles the OnResourceUpdate event. </summary>
    private void OnResourceUpdate()
    {
        PluginLog.Debug($"ResourceManager: Resources updated.");

        Setup(PluginService.PluginInterface.UiLanguage);
    }


    /// <summary> Sets up the plugin's resources. </summary>
    private void Setup(string language)
    {
        PluginLog.Debug($"ResourceManager: Setting up resources for language {language}...");

        try { Loc.Setup(File.ReadAllText($"{PStrings.localizationPath}\\Plugin\\{language}.json")); }
        catch { Loc.SetupWithFallbacks(); }

        PluginLog.Debug("ResourceManager: Resources setup.");
    }
}