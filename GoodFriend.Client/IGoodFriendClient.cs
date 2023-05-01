using System;
using System.Threading.Tasks;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using RestSharp;

namespace GoodFriend.Client
{
    /// <summary>
    ///     Represents a client that can interact with the GoodFriend API.
    /// </summary>
    public interface IGoodFriendClient : IDisposable
    {
        /// <summary>
        ///     The base URI of all API endpoints, should include version if applicable.
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        ///     The current connection state of the client to the server-sent event stream.
        /// </summary>
        EventStreamConnectionState ConnectionState { get; }

        /// <summary>
        ///     Sends the players login state to the API.
        /// </summary>
        RestResponse SendLoginState(UpdatePlayerLoginStateRequest.PutData requestData);

        /// <inheritdoc cref="SendLogin(UpdatePlayerLoginStateRequest)" />
        Task<RestResponse> SendLoginStateAsync(UpdatePlayerLoginStateRequest.PutData requestData);

        /// <summary>
        ///     Gets metadata from the API.
        /// </summary>
        /// <returns>A <see cref="MetadataResponse" /> instance.</returns>
        MetadataResponse GetMetadata();

        /// <inheritdoc cref="GetMetadata" />
        Task<MetadataResponse> GetMetadataAsync();

        /// <summary>
        ///     Gets the motd from the API.
        /// </summary>
        /// <returns>A <see cref="MotdResponse" /> instance</returns>
        MotdResponse GetMotd();

        /// <inheritdoc cref="GetMotd" />
        Task<MotdResponse> GetMotdAsync();

        /// <summary>
        ///     Starts listening to the event stream.
        /// </summary>
        void ConnectToEventStream();

        /// <summary>
        ///     Stops listening to the event stream.
        /// </summary>
        void DisconnectFromEventStream();
    }
}