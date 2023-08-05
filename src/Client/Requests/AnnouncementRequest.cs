using System;
using System.Text.Json.Serialization;
using GoodFriend.Client.Responses.Enums;

namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents a request for a stream of announcements.
    /// </summary>
    public static class AnnouncementRequest
    {
        /// <summary>
        ///     The endpoint url without a proceeding slash.
        /// </summary>
        internal const string EndpointUrl = "api/announcements/send";

        [Serializable]
        public readonly record struct HttpPost
        {
            /// <summary>
            ///     The authentication token to use with the request.
            /// </summary>
            [JsonIgnore]
            public string AuthenticationToken { get; init; }

            /// <summary>
            ///     The message to send.
            /// </summary>
            public required string Message { get; init; }

            /// <summary>
            ///     The kind of announcement to send.
            /// </summary>
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public required AnnouncementKind Kind { get; init; }

            /// <summary>
            ///     The cause of the announcement.
            /// </summary>
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public required AnnouncementCause Cause { get; init; }

            /// <summary>
            ///     The target of the announcement.
            /// </summary>
            public string? Target { get; init; }
        }
    }
}