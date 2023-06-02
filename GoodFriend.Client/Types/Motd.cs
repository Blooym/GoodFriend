using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Types
{
    /// <summary>
    ///     Represents a response from the motd endpoint.
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
        ///     Whether the message is important or not.
        /// </summary>
        [JsonPropertyName("important")]
        public bool Important { get; init; }

        /// <summary>
        ///     Whether or not the API wants the client to ignore this message.
        /// </summary>
        [JsonPropertyName("ignore")]
        public readonly bool Ignore { get; init; }
    }
}