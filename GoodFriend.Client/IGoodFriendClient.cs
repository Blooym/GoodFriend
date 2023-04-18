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
        /// <param name="contentId">The unhashed player content id</param>
        /// <param name="datacenterId">The datacenter id of the player</param>
        /// <param name="worldId">The world id of the player</param>
        /// <param name="territoryId">The territory id of the player</param>
        /// <returns>A RestResponse for the request.</returns>
        bool SendLogin(ulong contentId, uint datacenterId, uint worldId, uint territoryId);

        /// <inheritdoc cref="SendLogin(ulong, uint, uint, uint)" />
        Task<bool> SendLoginAsync(ulong contentId, uint datacenterId, uint worldId, uint territoryId);

        /// <summary>
        ///     Sends a logout event to the api.
        /// </summary>
        /// <param name="contentId">The unhashed player content id</param>
        /// <param name="datacenterId">The datacenter id of the player</param>
        /// <param name="worldId">The world id of the player</param>
        /// <param name="territoryId">The territory id of the player</param>
        /// <returns>A RestResponse for the request.</returns>
        bool SendLogout(ulong contentId, uint datacenterId, uint worldId, uint territoryId);

        /// <inheritdoc cref="SendLogout(ulong, uint, uint, uint)" />
        Task<bool> SendLogoutAsync(ulong contentId, uint datacenterId, uint worldId, uint territoryId);

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