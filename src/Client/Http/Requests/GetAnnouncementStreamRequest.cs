using System.Net.Http;
using System.Timers;
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
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="reconnectTimer">The timer to use for reconnecting, if any.</param>
        /// <returns>A new SSE client for the announcement stream.</returns>
        public static SseClient<AnnouncementStreamUpdate> CreateSSEClient(HttpClient httpClient, Timer? reconnectTimer) => new(httpClient, EndpointUrl, reconnectTimer);
    }
}
