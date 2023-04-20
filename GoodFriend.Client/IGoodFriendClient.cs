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
    internal interface IGoodFriendClient
    {
        /// <summary>
        ///     The base uri of all api endpoints, should include version if applicable.
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        ///     The current connection state of the client to the server-sent event stream.
        /// </summary>
        EventStreamConnectionState ConnectionState { get; }

        /// <summary>
        ///     Sends a login event to the api.
        /// </summary>
        RestResponse SendLogin(LoginRequest requestData);

        /// <inheritdoc cref="SendLogin(LoginRequest)" />
        Task<RestResponse> SendLoginAsync(LoginRequest requestData);

        /// <summary>
        ///     Sends a logout event to the api.
        /// </summary>
        RestResponse SendLogout(LogoutRequest requestData);

        /// <inheritdoc cref="SendLogout(LogoutRequest)" />
        Task<RestResponse> SendLogoutAsync(LogoutRequest requestData);

        /// <summary>
        ///     Gets metadata from the api.
        /// </summary>
        /// <returns>A <see cref="MetadataResponse" /> instance.</returns>
        MetadataResponse GetMetadata();

        /// <inheritdoc cref="GetMetadata" />
        Task<MetadataResponse> GetMetadataAsync();

        /// <summary>
        ///     Gets the motd from the api.
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