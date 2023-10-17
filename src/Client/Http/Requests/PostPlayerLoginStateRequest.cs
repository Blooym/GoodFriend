using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GoodFriend.Client.Json;

namespace GoodFriend.Client.Http.Requests
{
    /// <summary>
    ///     Represents the request data for sending a player login state.
    /// </summary>
    public class PostPlayerLoginStateRequest : IGoodFriendRequestHandler<PostPlayerLoginStateRequest.RequestData, HttpResponseMessage>
    {
        private const string EndpointUrl = "api/playerevents/loginstate";

        [Serializable]
        public readonly record struct RequestData
        {
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
                    if (value.Length < RequestConstants.ContentIdHashMinLength)
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
                    if (value.Length < RequestConstants.ContentIdSaltMinLength)
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

        /// <summary>
        ///     Builds the request message.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildMessage(RequestData requestData) => new(HttpMethod.Post, EndpointUrl)
        {
            Content = JsonContent.Create(requestData, MediaTypeHeaderValue.Parse("application/json"), new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            }),
            Headers =
            {
                { RequestConstants.ContentIdHashHeader, requestData.ContentIdHash },
            },
        };

        /// <inheritdoc />
        public HttpResponseMessage Send(HttpClient httpClient, RequestData requestData)
        {
            var message = BuildMessage(requestData);
            return httpClient.Send(message);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAsync(HttpClient httpClient, RequestData requestData)
        {
            var message = BuildMessage(requestData);
            return httpClient.SendAsync(message);
        }
    }
}
