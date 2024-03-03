using System;
using System.Globalization;
using Dalamud.Configuration;
using GoodFriend.Plugin.Base;

namespace GoodFriend.Plugin.Configuration
{
    /// <summary>
    ///     Provides access to and determines the Plugin configuration.
    /// </summary>
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
            public static readonly Uri DefaultBaseUri = new("https://aether.blooym.dev/v3/");

            /// <summary>
            ///     The BaseURL to use when interacting with the API.
            /// </summary>
            /// <remarks>
            ///     This requires a restart to take effect.
            /// </remarks>
            public Uri BaseUrl { get; set; } = DefaultBaseUri;

            /// <summary>
            ///     The authentication key to use with authenticated requests.
            /// </summary>
            public string AuthKey { get; set; } = string.Empty;
        }

        /// <summary>
        ///     Saves the current configuration to disk.
        /// </summary>
        internal void Save() => DalamudInjections.PluginInterface.SavePluginConfig(this);

        /// <summary>
        ///     Loads the configuration from the disk or creates a new one if not found.
        /// </summary>
        /// <returns>The configuration instance.</returns>
        public static PluginConfiguration Load()
        {
            var config = DalamudInjections.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

            // Migrate api version if not up to date
            var apiBaseUrl = config.ApiConfig.BaseUrl.AbsoluteUri;
            if (apiBaseUrl.StartsWith("https://aether.blooym.dev", true, CultureInfo.InvariantCulture) && !apiBaseUrl.Contains("/v3/", StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.Information($"Updating API Base URL from old version {config.ApiConfig.BaseUrl} to new version {ApiConfiguration.DefaultBaseUri}");
                config.ApiConfig.BaseUrl = ApiConfiguration.DefaultBaseUri;
                config.Save();
            }
            return config;
        }
    }
}
