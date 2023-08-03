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
        EventStreamConnectionState PlayerEventStreamConnectionState { get; }

        /// <summary>
        ///     Sends the players login state to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendLoginState(UpdatePlayerLoginStateRequest.PostData requestData);

        /// <inheritdoc cref="SendLogin(UpdatePlayerLoginStateRequest)" />
        Task<HttpResponseMessage> SendLoginStateAsync(UpdatePlayerLoginStateRequest.PostData requestData);

        /// <summary>
        ///     Sends the players world change to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendWorldChange(UpdatePlayerCurrentWorldRequest.PostData requestData);

        /// <inheritdoc cref="SendWorldChange(UpdatePlayerWorldRequest.PutData) />
        Task<HttpResponseMessage> SendWorldChangeAsync(UpdatePlayerCurrentWorldRequest.PostData requestData);

        /// <summary>
        ///     Sends an announcement to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendAnnouncement(AnnouncementRequest.PostData requestData);

        /// <inheritdoc cref="SendAnnouncement(AnnouncementRequest.PostData)" />
        Task<HttpResponseMessage> SendAnnouncementAsync(AnnouncementRequest.PostData requestData);

        /// <summary>
        ///     Gets metadata from the API.
        /// </summary>
        /// <returns>A <see cref="MetadataResponse" /> instance.</returns>
        (MetadataResponse, HttpResponseMessage) GetMetadata();

        /// <inheritdoc cref="GetMetadata" />
        Task<(MetadataResponse, HttpResponseMessage)> GetMetadataAsync();

        /// <summary>
        ///     Gets the features the API supports.
        /// </summary>
        /// <returns>A <see cref="FeaturesResponse" /> instance.</returns>
        (FeaturesResponse, HttpResponseMessage) GetFeatures();

        /// <inheritdoc cref="GetFeatures" />
        Task<(FeaturesResponse, HttpResponseMessage)> GetFeaturesAsync();

        /// <summary>
        ///     Starts listening to the player event stream.
        /// </summary>
        void ConnectToPlayerEventStream();

        /// <summary>
        ///     Stops listening to the player event stream.
        /// </summary>
        void DisconnectFromPlayerEventStream();

        /// <summary>
        ///    Starts listening to the announcements stream.
        /// </summary>
        void ConnectToAnnouncementsStream();

        /// <summary>
        ///     Stops listening to the announcements stream.
        /// </summary>
        void DisconnectFromAnnouncementsStream();
    }
}
