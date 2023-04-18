namespace GoodFriend.Client.Constants
{
    public static class LogoutRequestConstants
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
    }
}