namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents a request for a stream of player events.
    /// </summary>
    public static class PlayerEventsRequest
    {
        /// <summary>
        /// The endpoint url without a proceeding slash.
        /// </summary>
        internal const string EndpointUrl = "api/playerevents/stream";
    }
}
