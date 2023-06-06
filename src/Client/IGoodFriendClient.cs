using System;
using System.Net.Http;
using System.Threading.Tasks;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;

namespace GoodFriend.Client
{
    /// <summary>
    ///     Represents a client that can interact with the GoodFriend API.
    /// </summary>
    public interface IGoodFriendClient : IDisposable
    {
        /// <summary>
        ///     The options used to configure the client.
        /// </summary>
        GoodfriendClientOptions Options { get; }

        /// <summary>
        ///     The current connection state of the client to player event stream.
        /// </summary>
        EventStreamConnectionState PlayerStreamConnectionState { get; }

        /// <summary>
        ///     Sends the players login state to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendLoginState(UpdatePlayerLoginStateRequest.PutData requestData);

        /// <inheritdoc cref="SendLogin(UpdatePlayerLoginStateRequest)" />
        Task<HttpResponseMessage> SendLoginStateAsync(UpdatePlayerLoginStateRequest.PutData requestData);

        /// <summary>
        ///     Sends the players world change to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendWorldChange(UpdatePlayerWorldRequest.PutData requestData);

        /// <inheritdoc cref="SendWorldChange(UpdatePlayerWorldRequest.PutData) />
        Task<HttpResponseMessage> SendWorldChangeAsync(UpdatePlayerWorldRequest.PutData requestData);

        /// <summary>
        ///     Gets metadata from the API.
        /// </summary>
        /// <returns>A <see cref="MetadataResponse" /> instance.</returns>
        (MetadataResponse, HttpResponseMessage) GetMetadata();

        /// <inheritdoc cref="GetMetadata" />
        Task<(MetadataResponse, HttpResponseMessage)> GetMetadataAsync();

        /// <summary>
        ///     Gets the minimum allowed game version from the API.
        /// </summary>
        /// <returns>A <see cref="MinimumGameVersionResponse" /> instance.</returns>
        (MinimumGameVersionResponse, HttpResponseMessage) GetMinimumVersion();

        /// <inheritdoc cref="GetMinimumVersion" />
        Task<(MinimumGameVersionResponse, HttpResponseMessage)> GetMinimumVersionAsync();

        /// <summary>
        ///     Starts listening to the player event stream.
        /// </summary>
        void ConnectToPlayerEventStream();

        /// <summary>
        ///     Stops listening to the player event stream.
        /// </summary>
        void DisconnectFromPlayerEventStream();

        /// <summary>
        ///     Updates the options used to configure the client.
        /// </summary>
        /// <param name="options">The new options.</param>
        void UpdateOptions(GoodfriendClientOptions options);
    }
}
