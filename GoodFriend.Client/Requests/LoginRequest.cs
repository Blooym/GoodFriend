namespace GoodFriend.Client.Requests
{
    public readonly struct LoginRequest
    {
        /// <summary>
        ///     The endpoint url with a proceeding slash.
        /// </summary>
        public const string EndpointUrl = "/login";
        /// <summary>
        ///     The datacenter id query parameter name.
        /// </summary>
        public const string DatacenterIdParam = "datacenter_id";

        /// <summary>
        ///     The world id query parameter name.
        /// </summary>
        public const string WorldIdParam = "world_id";

        /// <summary>
        ///     The territory id query parameter name.
        /// </summary>
        public const string TerritoryIdParam = "territory_id";

        /// <summary>
        ///     The content id query parameter name.
        /// </summary>
        public const string ContentIdParam = "content_id_hash";

        /// <summary>
        ///     The content id salt parameter name.
        /// </summary>
        public const string ContentIdSaltParam = "content_id_salt";

        public required string ContentIdHash { get; init; }
        public required string ContentIdSalt { get; init; }
        public required uint DatacenterId { get; init; }
        public required uint WorldId { get; init; }
        public required uint TerritoryId { get; init; }
    }
}