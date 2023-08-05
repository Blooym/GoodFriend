using System;
using System.Net.Http;
using System.Threading.Tasks;
using GoodFriend.Client.Http;
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
        ///     The announcement stream.
        /// </summary>
        SSEClient<AnnouncementStreamUpdate> AnnouncementStream { get; }

        /// <summary>
        ///     The player event stream.
        /// </summary>
        SSEClient<PlayerEventStreamUpdate> PlayerEventStream { get; }

        /// <summary>
        ///     Sends the players login state to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendLoginState(UpdatePlayerLoginStateRequest.HttpPost requestData);

        /// <inheritdoc cref="SendLogin(UpdatePlayerLoginStateRequest)" />
        Task<HttpResponseMessage> SendLoginStateAsync(UpdatePlayerLoginStateRequest.HttpPost requestData);

        /// <summary>
        ///     Sends the players world change to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendWorldChange(UpdatePlayerCurrentWorldRequest.HttpPost requestData);

        /// <inheritdoc cref="SendWorldChange(UpdatePlayerWorldRequest.PutData) />
        Task<HttpResponseMessage> SendWorldChangeAsync(UpdatePlayerCurrentWorldRequest.HttpPost requestData);

        /// <summary>
        ///     Sends an announcement to the API.
        /// </summary>
        /// <param name="requestData">The data to send.</param>
        /// <returns>A <see cref="RestResponse" /> instance.</returns>
        HttpResponseMessage SendAnnouncement(AnnouncementRequest.HttpPost requestData);

        /// <inheritdoc cref="SendAnnouncement(AnnouncementRequest.HttpPost)" />
        Task<HttpResponseMessage> SendAnnouncementAsync(AnnouncementRequest.HttpPost requestData);

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
    }
}
