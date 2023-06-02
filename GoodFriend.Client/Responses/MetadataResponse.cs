using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GoodFriend.Client.Types;

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
}