using System;
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
        ///     The status page URL.
        /// </summary>
        [JsonPropertyName("status_url")]
        public readonly string StatusUrl { get; init; }

        /// <summary>
        ///     The donation page URL.
        /// </summary>
        [JsonPropertyName("donate_url")]
        public readonly string DonateUrl { get; init; }
    }
}