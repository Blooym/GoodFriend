using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    [Serializable]
    public readonly record struct MotdResponse
    {
        /// <summary>
        ///     The text content of the "message of the day".
        /// </summary>
        [JsonPropertyName("content")]
        public readonly string Content { get; init; }

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