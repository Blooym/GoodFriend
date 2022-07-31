namespace GoodFriend.Managers;

using System;
using System.Net;
using System.Threading;
using System.IO;
using System.IO.Compression;
using GoodFriend.Base;
using Dalamud.Logging;

/// <summary>
///    The UpdateManager class is responsible for managing the update processes for the plugin.
/// </summary>
internal static class UpdateManager
{
    /// <summary> If the last update was a success or not. </summary>
    internal static bool? lastUpdateSuccess;

    /// <summary> If an update is currently in progress. </summary>
    internal static bool updateInProgress;

    /// <summary> Broadcasted when the plugin's resources have been updated.</summary>
    internal static event ResourceUpdateDelegate? ResourcesUpdated;
    internal delegate void ResourceUpdateDelegate();

    /// <summary> Downloads the repository from GitHub and extracts the resource data. </summary>
    internal static void UpdateResources()
    {
        // To prevent blocking the main thread, we'll use a background thread.
        Thread downloadThread = new Thread(() =>
        {

            try
            {
                PluginLog.Debug($"UpdateManager: Opening new thread to handle duty data download.");
                updateInProgress = true;

                // Create a new WebClient to download the data and some paths for installation.
                var webClient = new WebClient();
                var zipFile = Path.Combine(Path.GetTempPath(), "GoodFriend.zip");
                var sourcePath = Path.Combine(Path.GetTempPath(), "GoodFriend-main", "src", "Resources");
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
                GoodFriendPlugin.OnLanguageChange(Service.PluginInterface.UiLanguage);
            }
            catch (Exception e)
            {
                // Set update statuses to their values & log the error.
                lastUpdateSuccess = false;
                updateInProgress = false;
                PluginLog.Error($"UpdateManager: Error updating resource files: {e.Message}");
            }

        });

        downloadThread.Start();
    }
}