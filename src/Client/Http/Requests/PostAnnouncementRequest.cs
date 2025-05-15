using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GoodFriend.Client.Http.Enums;
using GoodFriend.Client.Http.Interfaces;
using GoodFriend.Client.Json;

namespace GoodFriend.Client.Http.Requests;

public sealed class PostAnnouncementRequest : IHttpRequestHandler<PostAnnouncementRequest.RequestData, HttpResponseMessage>
{
    private const string EndpointUrl = "api/announcements/send";

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

    private static HttpRequestMessage BuildMessage(RequestData requestData) => new(HttpMethod.Post, EndpointUrl)
    {
        Content = JsonContent.Create(requestData, MediaTypeHeaderValue.Parse("application/json"), new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        }),
        Headers =
        {
            { GlobalRequestData.Headers.AuthenticationToken, requestData.AuthenticationToken },
        },
    };

    /// <inheritdoc/>
    public HttpResponseMessage Send(HttpClient httpClient, RequestData requestData) => httpClient.Send(BuildMessage(requestData));

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendAsync(HttpClient httpClient, RequestData requestData) => httpClient.SendAsync(BuildMessage(requestData));
}
