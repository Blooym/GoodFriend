using System;

namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents a request to update the players login state.
    /// </summary>
    public static class UpdatePlayerLoginStateRequest
    {
        /// <summary>
        ///     The endpoint url with a proceeding slash.
        /// </summary>
        internal const string EndpointUrl = "api/update/loginstate";

        public readonly record struct PutData
        {
            private const uint ContentIdHashMinLength = GlobalRequestData.ContentIdHashMinLength;
            private const uint ContentIdSaltMinLength = GlobalRequestData.ContentIdSaltMinLength;

            /// <summary>
            ///     The content id query parameter name.
            /// </summary>
            internal const string ContentIdParam = "content_id_hash";

            /// <summary>
            ///     The content id salt query parameter name.
            /// </summary>
            internal const string ContentIDSaltParam = "content_id_salt";

            /// <summary>
            ///     The datacenter id query parameter name.
            /// </summary>
            internal const string DatacenterIdParam = "datacenter_id";

            /// <summary>
            ///     The world id query parameter name.
            /// </summary>
            internal const string WorldIdParam = "world_id";

            /// <summary>
            ///     The territory id query parameter name.
            /// </summary>
            internal const string TerritoryIdParam = "territory_id";

            /// <summary>
            ///     The "is logged in" query parameter name.
            /// </summary>
            internal const string LoggedInParam = "logged_in";

            private readonly string contentIdHashBackingField;

            /// <summary>
            ///     The string of a hashed player ContentId.
            /// </summary>
            /// <remarks>
            ///     - The given string must be at least 64 characters in length. <br/>
            ///     - You cannot use the same hash across requests and must generate a new one each time. <br/>
            /// </remarks>
            public required string ContentIdHash
            {
                get => this.contentIdHashBackingField; init
                {
                    if (value.Length < ContentIdHashMinLength)
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
                    if (value.Length < ContentIdSaltMinLength)
                    {
                        throw new ArgumentException("ContentIdSalt must be at least 32 characters in length");
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

            /// <summary>
            ///     Whether or not the player is now logged in.
            /// </summary>
            public required bool LoggedIn { get; init; }
        }
    }
}
