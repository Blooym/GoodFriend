namespace GoodFriend.Client.Requests
{
    internal readonly record struct MotdRequest
    {
        /// <summary>
        ///     The endpoint url with a proceeding slash.
        /// </summary>
        public const string EndpointUrl = "/motd";
    }
}