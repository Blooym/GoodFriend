using System;
using System.Text.Json.Serialization;
using GoodFriend.Client.Responses.Enums;

namespace GoodFriend.Client.Responses
{
    /// <summary>
    ///     Represents a response from the announcement stream.
    /// </summary>
    [Serializable]
    public record struct AnnouncementStreamUpdate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("kind"), JsonConverter(typeof(JsonStringEnumConverter))]
        public AnnouncementKind Kind { get; set; }

        [JsonPropertyName("cause"), JsonConverter(typeof(JsonStringEnumConverter))]
        public AnnouncementCause Cause { get; set; }
    }
}
