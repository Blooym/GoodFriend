namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     constants for the events endpoint.
    /// </summary>
    public readonly record struct EventsRequest
    {
        /// <summary>
        /// The endpoint url with a proceeding slash.
        /// </summary>
        public const string EndpointUrl = "/events";
    }
}