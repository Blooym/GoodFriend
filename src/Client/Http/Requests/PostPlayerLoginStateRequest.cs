using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GoodFriend.Client.Http.Interfaces;
using GoodFriend.Client.Json;

namespace GoodFriend.Client.Http.Requests;

/// <summary>
///     Represents the request data for sending a player login state.
/// </summary>
public sealed class PostPlayerLoginStateRequest : IHttpRequestHandler<PostPlayerLoginStateRequest.RequestData, HttpResponseMessage>
{
    private const string EndpointUrl = "api/playerevents/loginstate";

    private readonly struct RequestBody
    {
        public required uint WorldId { get; init; }
        public required ushort TerritoryId { get; init; }
        public required bool LoggedIn { get; init; }
    }

    public readonly record struct RequestData
    {
        private readonly string contentIdHashBackingField;

        /// <summary>
        ///     The hash of the player's ContentId.
        /// </summary>
        /// <remarks>
        ///     - This value must be at least 64 characters in length. <br/>
        ///     - This value must be unique across every request.
        /// </remarks>
        public required string ContentIdHash
        {
            get => this.contentIdHashBackingField; init
            {
                if (value.Length < GlobalRequestData.Validation.ContentIdHashMinLength)
                {
                    throw new ArgumentException("ContentIdHash must be at least 64 characters in length");
                }
                this.contentIdHashBackingField = value;
            }
        }

        private readonly string contentIdSaltBackingField;

        /// <summary>
        ///     The salt used when hashing the player's ContentId.
        /// </summary>
        /// <remarks>
        ///     - This value must be at least 32 characters in length.
        /// </remarks>
        public required string ContentIdSalt
        {
            get => this.contentIdSaltBackingField; init
            {
                if (value.Length < GlobalRequestData.Validation.ContentIdSaltMinLength)
                {
                    throw new ArgumentException("ContentIdSalt must be at least 32 characters in length");
                }
                this.contentIdSaltBackingField = value;
            }
        }

        /// <summary>
        ///     The ID player's current World.
        /// </summary>
        public required uint WorldId { get; init; }

        /// <summary>
        ///     The ID of the player's current Territory.
        /// </summary>
        public required ushort TerritoryId { get; init; }

        /// <summary>
        ///     Whether the player is now logged in.
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
        Content = JsonContent.Create(new RequestBody()
        {
            LoggedIn = requestData.LoggedIn,
            TerritoryId = requestData.TerritoryId,
            WorldId = requestData.WorldId,
        }, MediaTypeHeaderValue.Parse("application/json"), new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        }),
        Headers =
        {
            { GlobalRequestData.Headers.ContentIdHash, requestData.ContentIdHash },
            { GlobalRequestData.Headers.ContentIdSalt, requestData.ContentIdSalt },
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
