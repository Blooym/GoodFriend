using System;
using System.Threading.Tasks;
using GoodFriend.Client.Responses;

namespace GoodFriend.Client
{
    /// <summary>
    ///     Represents a client that can interact with the GoodFriend API.
    /// </summary>
    public interface IGoodFriendClient
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
        /// <param name="identifier">The hashed player content id</param>
        /// <param name="datacenterId">The datacenter id of the player</param>
        /// <param name="worldId">The world id of the player</param>
        /// <param name="territoryId">The territory id of the player</param>
        /// <returns>A RestResponse for the request.</returns>
        bool SendLogin(string identifier, uint datacenterId, uint worldId, uint territoryId);

        /// <inheritdoc cref="SendLogin(string, uint, uint, uint)" />
        Task<bool> SendLoginAsync(string identifier, uint datacenterId, uint worldId, uint territoryId);

        /// <summary>
        ///     Sends a logout event to the api.
        /// </summary>
        /// <param name="identifier">The hashed player content id</param>
        /// <param name="datacenterId">The datacenter id of the player</param>
        /// <param name="worldId">The world id of the player</param>
        /// <param name="territoryId">The territory id of the player</param>
        /// <returns>A RestResponse for the request.</returns>
        bool SendLogout(string identifier, uint datacenterId, uint worldId, uint territoryId);

        /// <inheritdoc cref="SendLogout(string, uint, uint, uint)" />
        Task<bool> SendLogoutAsync(string identifier, uint datacenterId, uint worldId, uint territoryId);

        /// <summary>
        ///     Gets metadata from the api.
        /// </summary>
        /// <returns>A <see cref="MetadataResponse" /> instance.</returns>
        MetadataResponse GetMetadata();

        /// <inheritdoc cref="GetMetadata" />
        Task<MetadataResponse> GetMetadataAsync();

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