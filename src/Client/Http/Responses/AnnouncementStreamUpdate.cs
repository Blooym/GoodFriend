using System.Text.Json.Serialization;
using GoodFriend.Client.Http.Enums;

namespace GoodFriend.Client.Http.Responses;

/// <summary>
///     Represents a response from the announcement stream.
/// </summary>
public record struct AnnouncementStreamUpdate
{
    /// <summary>
    ///     The UUID of the announcement.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    ///     The message of the announcement.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    ///     The kind of the announcement.
    /// </summary>
    [JsonPropertyName("kind"), JsonConverter(typeof(JsonStringEnumConverter))]
    public AnnouncementKind Kind { get; set; }

    /// <summary>
    ///     The reason for the announcement.
    /// </summary>
    [JsonPropertyName("cause"), JsonConverter(typeof(JsonStringEnumConverter))]
    public AnnouncementCause Cause { get; set; }

    /// <summary>
    ///     The channel of the announcement.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }
}
