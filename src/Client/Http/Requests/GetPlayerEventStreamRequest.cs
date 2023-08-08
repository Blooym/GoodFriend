using System.Net.Http;
using System.Timers;
using GoodFriend.Client.Http.Responses;

namespace GoodFriend.Client.Http.Requests
{
    /// <summary>
    ///     Represents a request for the player event stream.
    /// </summary>
    public static class GetPlayerEventStreamRequest
    {
        private const string EndpointUrl = "api/playerevents/stream";

        /// <summary>
        ///     Creates a new SSE client for the player event stream using its endpoint URL.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="reconnectTimer">The timer to use for reconnecting, if any.</param>
        /// <returns></returns>
        public static SseClient<PlayerEventStreamUpdate> CreateSSEClient(HttpClient httpClient, Timer reconnectTimer) => new(httpClient, EndpointUrl, reconnectTimer);

    }
}
