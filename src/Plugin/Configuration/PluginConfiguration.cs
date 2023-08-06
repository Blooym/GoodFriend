using System;
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
            public static readonly Uri DefaultApiBaseUrl = new("https://aether.blooym.dev/v3/");

            /// <summary>
            ///     The BaseURL to use when interacting with the API.
            /// </summary>
            /// <remarks>
            ///     This requires a restart to take effect.
            /// </remarks>
            public Uri ApiBaseUrl { get; set; } = DefaultApiBaseUrl;

            /// <summary>
            ///     The API authorization token to use with authenticated requests.
            /// </summary>
            public string ApiKey { get; set; } = string.Empty;

            /// <summary>
            ///     Resets the BaseURL to the default.
            /// </summary>
            public void ResetApiBaseUrl() => this.ApiBaseUrl = DefaultApiBaseUrl;
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
            if (config.ApiConfig.ApiBaseUrl.AbsoluteUri.StartsWith("https://aether.blooym.dev", StringComparison.OrdinalIgnoreCase) && !config.ApiConfig.ApiBaseUrl.AbsoluteUri.EndsWith("/v3/", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Information($"Updating API Base URL from {config.ApiConfig.ApiBaseUrl} to {ApiConfiguration.DefaultApiBaseUrl}");
                config.ApiConfig.ApiBaseUrl = ApiConfiguration.DefaultApiBaseUrl;
            }

            return config;
        }
    }
}
