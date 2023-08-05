using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    /// <summary>
    ///     Represents response from the metadata endpoint.
    /// </summary>
    [Serializable]
    public readonly record struct MetadataResponse
    {
        /// <summary>
        ///     The connections to the API.
        /// </summary>
        [JsonPropertyName("connections")]
        public readonly Connections Connections { get; init; }

        /// <summary>
        ///     The about section of the API.
        /// </summary>
        [JsonPropertyName("about")]
        public readonly About About { get; init; }
    }

    /// <summary>
    ///     Represents the about section of metadata.
    /// </summary>
    [Serializable]
    public readonly record struct About
    {
        /// <summary>
        ///     The identifier or "name" of the API, purely cosmetic.
        /// </summary>
        [JsonPropertyName("identifier")]
        public readonly string Identifier { get; init; }

        /// <summary>
        ///     The URL to the banner of the API.
        /// </summary>
        [JsonPropertyName("banner_url")]
        public readonly Uri? BannerUrl { get; init; }

        /// <summary>
        ///     The description of the API.
        /// </summary>
        [JsonPropertyName("description")]
        public readonly string Description { get; init; }

        /// <summary>
        ///     Custom URLs.
        /// </summary>
        [JsonPropertyName("custom_urls")]
        public readonly Dictionary<string, Uri> CustomUrls { get; init; }
    }

    [Serializable]
    public readonly record struct Connections
    {
        [JsonPropertyName("player_events")]
        public readonly uint PlayerEvents { get; init; }

        [JsonPropertyName("announcements")]
        public readonly uint Announcements { get; init; }
    }
}
