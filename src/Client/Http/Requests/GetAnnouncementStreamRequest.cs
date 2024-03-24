using System.Net.Http;
using GoodFriend.Client.Http.Responses;

namespace GoodFriend.Client.Http.Requests
{
    /// <summary>
    ///     Represents a request for the announcement stream.
    /// </summary>
    public static class GetAnnouncementStreamRequest
    {
        private const string EndpointUrl = "api/announcements/stream";

        /// <summary>
        ///     Creates a new SSE client for the announcement stream using its endpoint URL.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests. This must be a unique client as it will be managed once initialized.</param>
        /// <param name="settings">The settings for this client.</param>
        /// <returns>A new SSE client for the announcement stream.</returns>
        public static SseClient<AnnouncementStreamUpdate> CreateSSEClient(HttpClient httpClient, SSEClientSettings settings) => new(httpClient, EndpointUrl, settings);
    }
}
