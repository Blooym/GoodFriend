using System;

namespace GoodFriend.Client.Requests
{
    public readonly record struct LogoutRequest
    {
        /// <summary>
        ///     The endpoint url with a proceeding slash.
        /// </summary>
        public const string EndpointUrl = "/login";

        /// <summary>
        ///     The content id query parameter name.
        /// </summary>
        public const string ContentIdParam = "content_id_hash";

        /// <summary>
        ///     The content id salt parameter name.
        /// </summary>
        public const string ContentIdSaltParam = "content_id_salt";

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

        private readonly string contentIdHashBackingField;

        /// <summary>
        ///     The hex string of a hashed player ContentId.
        /// </summary>
        /// <remarks>
        ///     The given hex string must be at least 128 characters in length.
        /// </remarks>
        public required string ContentIdHash
        {
            get => this.contentIdHashBackingField; init
            {
                if (value.Length < 128)
                {
                    throw new ArgumentException("ContentIdHash must be a hex string at least 128 characters in length");
                }
                this.contentIdHashBackingField = value;
            }
        }

        private readonly string contentIdSaltBackingField;

        /// <summary>
        ///     The hex string of the salt used when hashing the player's ContentId.
        /// </summary>
        /// <remarks>
        ///     The given hex string must be at least 32 characters in length.
        /// </remarks>
        public required string ContentIdSalt
        {
            get => this.contentIdSaltBackingField; init
            {
                if (value.Length < 32)
                {
                    throw new ArgumentException("ContentIdSalt must be a hex string at least 32 characters in length");
                }
                this.contentIdSaltBackingField = value;
            }
        }

        /// <summary>
        ///     The player's current DatacenterID
        /// </summary>
        public required uint DatacenterId { get; init; }

        /// <summary>
        ///     The player's current WorldId.
        /// </summary>
        public required uint WorldId { get; init; }

        /// <summary>
        ///     The player's current TerritoryId.
        /// </summary>
        public required ushort TerritoryId { get; init; }
    }
}