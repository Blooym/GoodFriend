using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using GoodFriend.Client.Http;
using GoodFriend.Client.Json;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;

namespace GoodFriend.Client
{
    /// <summary>
    ///     A GoodFriend API-compatiable client.
    /// </summary>
    public sealed class GoodFriendClient : IGoodFriendClient
    {
        /// <summary>
        ///     The delay between reconnect attempts.
        /// </summary>
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(60);

        /// <summary>
        ///     The user-agent header value.
        /// </summary>
        private static readonly string UserAgent = $"GoodFriend.Client/{Assembly.GetExecutingAssembly().GetName().Version}";

        /// <summary>
        ///     The http client handler instance.
        /// </summary>
        private readonly HttpClientHandler httpClientHandler = new()
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = true,
        };

        /// <summary>
        ///     The http client instance.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <inheritdoc />
        public GoodfriendClientOptions Options { get; private set; }

        /// <inheritdoc />
        public SSEClient<AnnouncementStreamUpdate> AnnouncementStream { get; private set; }

        /// <inheritdoc />
        public SSEClient<PlayerEventStreamUpdate> PlayerEventStream { get; private set; }

        /// <summary>
        ///     Creates a new instance of the client.
        /// </summary>
        /// <param name="baseUri">The base uri of the api</param>
        /// <param name="gameVersion">The current ffxivgame.ver value</param>
        public GoodFriendClient(GoodfriendClientOptions options)
        {
            this.Options = options;
            this.httpClientHandler = CreateHttpClientHandler();
            this.httpClient = CreateHttpClient(this.httpClientHandler, options.BaseAddress);

            this.AnnouncementStream = new(CreateHttpClient(this.httpClientHandler, options.BaseAddress), AnnouncementStreamRequest.EndpointUrl, new Timer(ReconnectDelay));
            this.PlayerEventStream = new(CreateHttpClient(this.httpClientHandler, options.BaseAddress), PlayerEventsRequest.EndpointUrl, new Timer(ReconnectDelay));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                this.AnnouncementStream.Dispose();
                this.PlayerEventStream.Dispose();
            }
            catch
            {
                //
            }

            this.httpClient.CancelPendingRequests();
            this.httpClient.Dispose();
            this.httpClientHandler.Dispose();
        }

        /// <summary>
        ///     Creates a new custom <see cref="HttpClientHandler" />
        /// </summary>
        /// <returns></returns>
        private static HttpClientHandler CreateHttpClientHandler() => new()
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = true,
        };

        /// <summary>
        ///     Creates a new custom <see cref="HttpClient" /> with the given <paramref name="baseUri" />
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="gameVersion"></param>
        /// <returns></returns>
        private static HttpClient CreateHttpClient(HttpClientHandler handler, Uri baseUri) => new(handler)
        {
            BaseAddress = baseUri,
            DefaultRequestHeaders =
            {
                { "User-Agent", UserAgent },
            },
            Timeout = TimeSpan.FromSeconds(10),
        };

        /// <summary>
        ///     Builds a new login state update request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildLoginStatePostRequest(UpdatePlayerLoginStateRequest.HttpPost requestData)
        {
            var jsonBody = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            });
            return new HttpRequestMessage(HttpMethod.Post, UpdatePlayerLoginStateRequest.EndpointUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                Headers =
                {
                    { UpdatePlayerLoginStateRequest.HttpPost.ContentIdHashHeader, requestData.ContentIdHash },
                },
            };
        }

        /// <summary>
        ///     Builds a new world change request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildWorldChangePostRequest(UpdatePlayerCurrentWorldRequest.HttpPost requestData)
        {
            var jsonBody = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            });
            return new HttpRequestMessage(HttpMethod.Put, UpdatePlayerCurrentWorldRequest.EndpointUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                Headers =
                {
                    { UpdatePlayerCurrentWorldRequest.HttpPost.ContentIdHashHeader, requestData.ContentIdHash },
                },
            };
        }

        /// <summary>
        ///     Builds a new metadata request.
        /// </summary>
        /// <returns></returns>
        private static HttpRequestMessage BuildMetadataRequest() => new(HttpMethod.Get, MetadataRequest.EndpointUrl);

        /// <summary>
        ///     Builds a new features request.
        /// </summary>
        /// <returns></returns>
        private static HttpRequestMessage BuildFeaturesRequest() => new(HttpMethod.Get, FeaturesRequest.EndpointUrl);

        /// <summary>
        ///     Builds a new send announcement request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildSendAnnouncementRequest(AnnouncementRequest.HttpPost requestData)
        {
            var jsonBody = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            });
            return new HttpRequestMessage(HttpMethod.Post, AnnouncementRequest.EndpointUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                Headers =
                {
                    { GlobalRequestData.AuthenticationTokenHeader, requestData.AuthenticationToken },
                },
            };
        }

        /// <summary>
        ///     Builds a new validate authentication token request with the given <paramref name="token" />
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildValidateAuthenticationTokenRequest(string token) => new(HttpMethod.Get, ValidateAuthRequest.EndpointUrl)
        {
            Headers =
            {
                { GlobalRequestData.AuthenticationTokenHeader, token },
            },
        };

        /// <inheritdoc />
        public HttpResponseMessage SendLoginState(UpdatePlayerLoginStateRequest.HttpPost requestData)
        {
            var request = BuildLoginStatePostRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendLoginStateAsync(UpdatePlayerLoginStateRequest.HttpPost requestData)
        {
            var request = BuildLoginStatePostRequest(requestData);
            return this.httpClient.SendAsync(request);
        }

        /// <inheritdoc />
        public HttpResponseMessage SendWorldChange(UpdatePlayerCurrentWorldRequest.HttpPost requestData)
        {
            var request = BuildWorldChangePostRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendWorldChangeAsync(UpdatePlayerCurrentWorldRequest.HttpPost requestData)
        {
            var request = BuildWorldChangePostRequest(requestData);
            return this.httpClient.SendAsync(request);
        }

        /// <inheritdoc />
        public (MetadataResponse, HttpResponseMessage) GetMetadata()
        {
            var request = BuildMetadataRequest();
            var response = this.httpClient.Send(request);
            return (response.Content.ReadFromJsonAsync<MetadataResponse>().Result, response);
        }

        /// <inheritdoc />
        public async Task<(MetadataResponse, HttpResponseMessage)> GetMetadataAsync()
        {
            var request = BuildMetadataRequest();
            var response = await this.httpClient.SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<MetadataResponse>(), response);
        }

        /// <inheritdoc />
        public (FeaturesResponse, HttpResponseMessage) GetFeatures()
        {
            var request = BuildFeaturesRequest();
            var response = this.httpClient.Send(request);
            return (response.Content.ReadFromJsonAsync<FeaturesResponse>().Result, response);
        }

        /// <inheritdoc />
        public async Task<(FeaturesResponse, HttpResponseMessage)> GetFeaturesAsync()
        {
            var request = BuildFeaturesRequest();
            var response = await this.httpClient.SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<FeaturesResponse>(), response);
        }

        /// <inheritdoc />
        public HttpResponseMessage SendAnnouncement(AnnouncementRequest.HttpPost requestData)
        {
            var request = BuildSendAnnouncementRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAnnouncementAsync(AnnouncementRequest.HttpPost requestData)
        {
            var request = BuildSendAnnouncementRequest(requestData);
            return this.httpClient.SendAsync(request);
        }

        /// <inheritdoc />
        public (bool, HttpResponseMessage) ValidateAuthToken(string token)
        {
            var request = BuildValidateAuthenticationTokenRequest(token);
            var response = this.httpClient.Send(request);
            return (response.StatusCode == HttpStatusCode.OK, response);
        }

        /// <inheritdoc />
        public async Task<(bool, HttpResponseMessage)> ValidateAuthTokenAsync(string token)
        {
            var request = BuildValidateAuthenticationTokenRequest(token);
            var response = await this.httpClient.SendAsync(request);
            return (response.StatusCode == HttpStatusCode.OK, response);
        }
    }

    /// <summary>
    ///     Represents the options for a <see cref="GoodfriendClient"/>.
    /// </summary>
    public readonly struct GoodfriendClientOptions
    {
        /// <summary>
        ///     The base address of the Goodfriend API.
        /// </summary>
        public readonly required Uri BaseAddress { get; init; }

        public GoodfriendClientOptions()
        {
        }
    }
}
