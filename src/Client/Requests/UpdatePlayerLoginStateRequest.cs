using System;

namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents a request to update the players login state.
    /// </summary>
    public static class UpdatePlayerLoginStateRequest
    {
        /// <summary>
        ///     The endpoint url without a proceeding slash.
        /// </summary>
        internal const string EndpointUrl = "api/playerevents/loginstate";

        [Serializable]
        public readonly record struct HttpPost
        {
            /// <summary>
            ///     The name of the header to send the ContentIdHash in.
            /// </summary>
            internal const string ContentIdHashHeader = "X-Content-Id-Hash";

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
                    if (value.Length < GlobalRequestData.ContentIdHashMinLength)
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
                    if (value.Length < GlobalRequestData.ContentIdSaltMinLength)
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
