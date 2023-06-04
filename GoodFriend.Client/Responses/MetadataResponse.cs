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
        ///     The current number of connected clients.
        /// </summary>
        [JsonPropertyName("connected_clients")]
        public readonly uint ConnectedClients { get; init; }

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
        public readonly Uri BannerUrl { get; init; }

        /// <summary>
        ///     The message of the day.
        /// </summary>
        [JsonPropertyName("motd")]
        public readonly Motd Motd { get; init; }

        /// <summary>
        ///     Custom URLs.
        /// </summary>
        [JsonPropertyName("custom_urls")]
        public readonly Dictionary<string, Uri> CustomUrls { get; init; }
    }

    /// <summary>
    ///     Represents a message of the day.
    /// </summary>
    [Serializable]
    public readonly record struct Motd
    {
        /// <summary>
        ///     The message content of the "message of the day".
        /// </summary>
        [JsonPropertyName("message")]
        public readonly string Message { get; init; }

        /// <summary>
        ///     Whether or not the API wants the client to ignore this message.
        /// </summary>
        [JsonPropertyName("ignore")]
        public readonly bool Ignore { get; init; }
    }
}
