using System;
using System.IO;
using Dalamud.Configuration;
using GoodFriend.Plugin.Base;

namespace GoodFriend.Plugin.Configuration
{
    /// <summary>
    ///     Provides access to and determines the Plugin configuration.
    /// </summary>
    [Serializable]
    internal sealed class PluginConfiguration : IPluginConfiguration
    {
        /// <summary>
        ///     The current configuration version, incremented on breaking changes.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        ///     The configuration for the API.
        /// </summary>
        public ApiConfiguration ApiConfig { get; set; } = new();

        /// <summary>
        ///     The configuration for the API.
        /// </summary>
        public sealed class ApiConfiguration
        {
            /// <summary>
            ///     The default API URL to use.
            /// </summary>
            [NonSerialized]
            public static readonly Uri DefaultApiBaseUrl = new("https://aether.blooym.dev/");

            /// <summary>
            ///     The BaseURL to use when interacting with the API.
            /// </summary>
            /// <remarks>
            ///     This requires a restart to take effect.
            /// </remarks>
            public Uri ApiBaseUrl { get; set; } = DefaultApiBaseUrl;

            /// <summary>
            ///     Resets the BaseURL to the default.
            /// </summary>
            public void ResetApiBaseUrl() => this.ApiBaseUrl = DefaultApiBaseUrl;
        }

        /// <summary>
        ///     Saves the current configuration to disk.
        /// </summary>
        internal void Save()
        {
            var path = Path.Combine(DalamudInjections.PluginInterface.ConfigDirectory.FullName, "GoodFriend.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            DalamudInjections.PluginInterface.SavePluginConfig(this);
        }
    }
}
