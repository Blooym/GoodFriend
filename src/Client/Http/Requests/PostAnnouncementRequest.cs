using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GoodFriend.Client.Http.Responses.Enums;
using GoodFriend.Client.Json;

namespace GoodFriend.Client.Http.Requests
{
    public class PostAnnouncementRequest : IGoodFriendRequestHandler<PostAnnouncementRequest.RequestData, HttpResponseMessage>
    {
        private const string EndpointUrl = "api/announcements/send";

        [Serializable]
        public readonly record struct RequestData
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
            ///     The channel for the announcement.
            /// </summary>
            public string? Channel { get; init; }
        }

        private static HttpRequestMessage BuildMessage(RequestData requestData)
        {
            var jsonBody = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            });
            return new HttpRequestMessage(HttpMethod.Post, EndpointUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                Headers =
                {
                    { RequestConstants.AuthenticationTokenHeader, requestData.AuthenticationToken },
                },
            };
        }

        /// <inheritdoc/>
        public HttpResponseMessage Send(HttpClient httpClient, RequestData requestData) => httpClient.Send(BuildMessage(requestData));

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendAsync(HttpClient httpClient, RequestData requestData) => httpClient.SendAsync(BuildMessage(requestData));
    }
}
