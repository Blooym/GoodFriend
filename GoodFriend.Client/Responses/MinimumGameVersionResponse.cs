using System;
using System.Text.Json.Serialization;
using GoodFriend.Client.Types;

namespace GoodFriend.Client.Responses
{
    /// <summary>
    ///     Represents a response from the minimum version endpoint.
    /// </summary>
    [Serializable]
    public readonly record struct MinimumGameVersionResponse
    {
        /// <summary>
        ///     The API's minimum allowed game version.
        /// </summary>
        [JsonPropertyName("version")]
        public readonly GameVersion GameVersion { get; init; }

        /// <summary>
        ///     The API's minimum allowed game version as a string.
        /// </summary>
        [JsonPropertyName("version_string")]
        public readonly string GameVersionString { get; init; }
    }
}