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

        public readonly record struct PostData { }
    }
}