using System;

namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents a request to update the players world.
    /// </summary>
    public static class UpdatePlayerWorldRequest
    {
        /// <summary>
        ///     The endpoint url with a proceeding slash.
        /// </summary>
        internal const string ENDPOINT_URL = "/api/update/world";

        public readonly record struct PutData
        {
            private const uint CONTENT_ID_HASH_MIN_LENGTH = GlobalRequestData.CONTENT_ID_HASH_MIN_LENGTH;
            private const uint CONTENT_ID_SALT_MIN_LENGTH = GlobalRequestData.CONTENT_ID_SALT_MIN_LENGTH;

            /// <summary>
            ///     The content id query parameter name.
            /// </summary>
            internal const string CONTENT_ID_PARAM = "content_id_hash";

            /// <summary>
            ///     The content id salt query parameter name.
            /// </summary>
            internal const string CONTENT_ID_SALT_PARAM = "content_id_salt";

            /// <summary>
            ///     The world id query parameter name.
            /// </summary>
            internal const string WORLD_ID_PARAM = "world_id";

            private readonly string contentIdHashBackingField;

            /// <summary>
            ///     The string of a hashed player ContentId.
            /// </summary>
            /// <remarks>
            ///     The given string must be at least 64 characters in length.
            /// </remarks>
            public required string ContentIdHash
            {
                get => this.contentIdHashBackingField; init
                {
                    if (value.Length < CONTENT_ID_HASH_MIN_LENGTH)
                    {
                        throw new ArgumentException("ContentIdHash must be at least 64 characters in length");
                    }
                    this.contentIdHashBackingField = value;
                }
            }

            private readonly string contentIdSaltBackingField;

            /// <summary>
            ///     The salt used to hash the player's ContentId.
            /// </summary>
            /// <remarks>
            ///     The given string must be at least 32 characters in length.
            /// </remarks>
            public required string ContentIdSalt
            {
                get => this.contentIdSaltBackingField; init
                {
                    if (value.Length < CONTENT_ID_SALT_MIN_LENGTH)
                    {
                        throw new ArgumentException("ContentIdSalt must be at least 32 characters in length");
                    }
                    this.contentIdSaltBackingField = value;
                }
            }

            /// <summary>
            ///     The player's new world id.
            /// </summary>
            public required uint WorldId { get; init; }
        }
    }
}