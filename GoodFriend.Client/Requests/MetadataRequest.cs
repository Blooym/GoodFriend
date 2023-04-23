namespace GoodFriend.Client.Requests
{
    internal readonly record struct MetadataRequest
    {
        /// <summary>
        ///     The endpoint url with a proceeding slash.
        /// </summary>
        public const string EndpointUrl = "/metadata";
    }
}