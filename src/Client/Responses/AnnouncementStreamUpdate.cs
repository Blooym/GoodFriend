using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    /// <summary>
    ///     The kind of announcement being made.
    /// </summary>
    public enum AnnouncementKind
    {
        Information,
        Maintenance,
        Update,
        Critical,
    }

    /// <summary>
    ///     The cause/reason for the announcement.
    /// </summary>
    public enum AnnouncementCause
    {
        Manual,
        Automatic,
        Scheduled,
    }

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
        public AnnouncementKind Cause { get; set; }
    }
}
