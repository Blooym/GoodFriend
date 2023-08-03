using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
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
        ///     The player event stream reconnect timer instance.
        /// </summary>
        private readonly Timer playerStreamReconnectTimer;

        /// <summary>
        ///     The announcement event stream reconnect timer instance.
        /// </summary>
        private readonly Timer announcementStreamReconnectTimer;

        /// <summary>
        ///     The http client instance.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        ///     Delegate for the <see cref="OnPlayerStreamMessage" /> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="update">Player update data.</param>
        public delegate void DelegatePlayerStateUpdate(object? sender, PlayerEventStreamUpdate update);

        /// <summary>
        ///     The event for when a player state update is received and is not a heartbeat.
        /// </summary>
        public event DelegatePlayerStateUpdate? OnPlayerStreamMessage;

        /// <summary>
        ///     Delegate for the <see cref="OnPlayerStreamException" /> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        public delegate void DelegatePlayerStreamError(object? sender, Exception exception);

        /// <summary>
        ///     The event for when an exception relating to the player event stream connection is received.
        /// </summary>
        /// <remarks>
        ///     Automatic reconnection is handled by the client and does not need to be implemented
        ///     by listening for this event.
        /// </remarks>
        public event DelegatePlayerStreamError? OnPlayerStreamException;

        /// <summary>
        ///     Delegate for the <see cref="OnPlayerStreamConnected" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegatePlayerStreamConnected(object? sender);

        /// <summary>
        ///     The event for when the player event stream has finished connecting successfully.
        /// </summary>
        public event DelegatePlayerStreamConnected? OnPlayerStreamConnected;

        /// <summary>
        ///     Delegate for the <see cref="OnPlayerStreamHeartbeat" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegatePlayerStreamHeartbeat(object? sender);

        /// <summary>
        ///     The event for when a heartbeat is received from the player event stream.
        /// </summary>
        public event DelegatePlayerStreamHeartbeat? OnPlayerStreamHeartbeat;

        /// <summary>
        ///     Delegate for the <see cref="OnPlayerStreamConnected" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegatePlayerStreamDisconnected(object? sender);

        /// <summary>
        ///     The event for when the player event stream has disconnected.
        /// </summary>
        /// <remarks>
        ///     Not fired when an exception is handled, only when the client disconnected properly.
        /// </remarks>
        public event DelegatePlayerStreamDisconnected? OnPlayerStreamDisconnected;

        /// <summary>
        ///     Delegate for the <see cref="OnAnnouncementStreamMessage" /> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="update">Announcement update data.</param>
        public delegate void DelegateAnnouncementStreamMessage(object? sender, AnnouncementStreamUpdate update);

        /// <summary>
        ///    The event for when an announcement update is received.
        /// </summary>
        public event DelegateAnnouncementStreamMessage? OnAnnouncementStreamMessage;

        /// <summary>
        ///     Delegate for the <see cref="OnAnnouncementStreamException" /> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        public delegate void DelegateAnnouncementStreamError(object? sender, Exception exception);

        /// <summary>
        ///     The event for when an exception relating to the announcement stream connection is received.
        /// </summary>
        public event DelegateAnnouncementStreamError? OnAnnouncementStreamException;

        /// <summary>
        ///     Delegate for the <see cref="OnAnnouncementStreamConnected" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegateAnnouncementStreamConnected(object? sender);

        /// <summary>
        ///     The event for when the announcement stream has finished connecting successfully.
        /// </summary>
        public event DelegateAnnouncementStreamConnected? OnAnnouncementStreamConnected;

        /// <summary>
        ///     Delegate for the <see cref="OnAnnouncementStreamHeartbeat" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegateAnnouncementStreamHeartbeat(object? sender);

        /// <summary>
        ///     The event for when a heartbeat is received from the announcement stream.
        /// </summary>
        public event DelegateAnnouncementStreamHeartbeat? OnAnnouncementStreamHeartbeat;

        /// <summary>
        ///     Delegate for the <see cref="OnAnnouncementStreamDisconnected" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegateAnnouncementStreamDisconnected(object? sender);

        /// <summary>
        ///     The event for when the announcement stream has disconnected.
        /// </summary>
        /// <remarks>
        ///    Not fired when an exception is handled, only when the client disconnected properly.
        /// </remarks>
        public event DelegateAnnouncementStreamDisconnected? OnAnnouncementStreamDisconnected;

        /// <inheritdoc />
        public EventStreamConnectionState PlayerEventStreamConnectionState { get; private set; } = EventStreamConnectionState.Disconnected;

        /// <inheritdoc />
        public EventStreamConnectionState AnnouncementStreamConnectionState { get; private set; } = EventStreamConnectionState.Disconnected;

        /// <inheritdoc />
        public GoodfriendClientOptions Options { get; private set; }

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

            this.playerStreamReconnectTimer = new Timer(options.ReconnectInterval);
            this.playerStreamReconnectTimer.Elapsed += this.HandlePlayerEventStreamTimerElapse;
            this.OnPlayerStreamException += this.HandlePlayerStreamException;
            this.OnPlayerStreamConnected += this.HandlePlayerStreamConnected;
            this.OnPlayerStreamDisconnected += this.HandlePlayerStreamDisconnected;

            this.announcementStreamReconnectTimer = new Timer(options.ReconnectInterval);
            this.announcementStreamReconnectTimer.Elapsed += this.HandleAnnouncementStreamTimerElapse;
            this.OnAnnouncementStreamException += this.HandleAnnouncementStreamException;
            this.OnAnnouncementStreamConnected += this.HandleAnnouncementStreamConnected;
            this.OnAnnouncementStreamDisconnected += this.HandleAnnouncementStreamDisconnected;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                this.DisconnectFromPlayerEventStream();
                this.DisconnectFromAnnouncementsStream();
            }
            catch
            {
                //
            }

            this.httpClient.CancelPendingRequests();
            this.httpClient.Dispose();
            this.httpClientHandler.Dispose();

            this.playerStreamReconnectTimer.Stop();
            this.playerStreamReconnectTimer.Dispose();
            this.playerStreamReconnectTimer.Elapsed -= this.HandlePlayerEventStreamTimerElapse;
            this.OnPlayerStreamException -= this.HandlePlayerStreamException;
            this.OnPlayerStreamConnected -= this.HandlePlayerStreamConnected;
            this.OnPlayerStreamDisconnected -= this.HandlePlayerStreamDisconnected;

            this.announcementStreamReconnectTimer.Stop();
            this.announcementStreamReconnectTimer.Dispose();
            this.announcementStreamReconnectTimer.Elapsed -= this.HandleAnnouncementStreamTimerElapse;
            this.OnAnnouncementStreamException -= this.HandleAnnouncementStreamException;
            this.OnAnnouncementStreamConnected -= this.HandleAnnouncementStreamConnected;
            this.OnAnnouncementStreamDisconnected -= this.HandleAnnouncementStreamDisconnected;
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
        ///     When the player event stream reconnect timer elapses, attempt to reconnect to the player event stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlePlayerEventStreamTimerElapse(object? sender, ElapsedEventArgs e)
        {
            // If we are not in an exception state, stop the timer and return.
            if (this.PlayerEventStreamConnectionState is not EventStreamConnectionState.Exception)
            {
                this.playerStreamReconnectTimer.Stop();
                return;
            }

            // Attempt to reconnect to the player event stream.
            this.ConnectToPlayerEventStream();
        }

        /// <summary>
        ///     Handles the player event stream exception event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void HandlePlayerStreamException(object? sender, Exception exception)
        {
            this.PlayerEventStreamConnectionState = EventStreamConnectionState.Exception;
            this.httpClient.CancelPendingRequests();
            this.playerStreamReconnectTimer.Start();
        }

        /// <summary>
        ///     Handles the player event stream connected event.
        /// </summary>
        /// <param name="sender"></param>
        private void HandlePlayerStreamConnected(object? sender) => this.playerStreamReconnectTimer.Stop();

        /// <summary>
        ///     Handles the player event stream disconnected event.
        /// </summary>
        /// <param name="sender"></param>
        private void HandlePlayerStreamDisconnected(object? sender) => this.playerStreamReconnectTimer.Stop();

        /// <summary>
        ///     When the announcement stream reconnect timer elapses, attempt to reconnect to the announcement event stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleAnnouncementStreamTimerElapse(object? sender, ElapsedEventArgs e)
        {
            // If we are not in an exception state, stop the timer and return.
            if (this.AnnouncementStreamConnectionState is not EventStreamConnectionState.Exception)
            {
                this.announcementStreamReconnectTimer.Stop();
                return;
            }

            // Attempt to reconnect to the announcement event stream.
            this.ConnectToAnnouncementsStream();
        }


        /// <summary>
        ///     Handles the announcement stream exception event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void HandleAnnouncementStreamException(object? sender, Exception exception)
        {
            this.AnnouncementStreamConnectionState = EventStreamConnectionState.Exception;
            this.httpClient.CancelPendingRequests();
            this.announcementStreamReconnectTimer.Start();
        }

        /// <summary>
        ///     Handles the announcement stream connected event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void HandleAnnouncementStreamConnected(object? sender) => this.announcementStreamReconnectTimer.Stop();

        /// <summary>
        ///     Handles the announcement stream disconnected event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void HandleAnnouncementStreamDisconnected(object? sender) => this.announcementStreamReconnectTimer.Stop();

        /// <summary>
        ///     Builds a new login state update request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildLoginStatePostRequest(UpdatePlayerLoginStateRequest.PostData requestData)
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
                    { UpdatePlayerLoginStateRequest.PostData.ContentIdHashHeader, requestData.ContentIdHash },
                },
            };
        }

        /// <summary>
        ///     Builds a new world change request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildWorldChangePostRequest(UpdatePlayerCurrentWorldRequest.PostData requestData)
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
                    { UpdatePlayerCurrentWorldRequest.PostData.ContentIdHashHeader, requestData.ContentIdHash },
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
        private static HttpRequestMessage BuildSendAnnouncementRequest(AnnouncementRequest.PostData requestData)
        {
            var jsonBody = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            });
            return new HttpRequestMessage(HttpMethod.Post, AnnouncementRequest.EndpointUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
            };
        }

        /// <inheritdoc />
        public HttpResponseMessage SendLoginState(UpdatePlayerLoginStateRequest.PostData requestData)
        {
            var request = BuildLoginStatePostRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendLoginStateAsync(UpdatePlayerLoginStateRequest.PostData requestData)
        {
            var request = BuildLoginStatePostRequest(requestData);
            return this.httpClient.SendAsync(request);
        }

        /// <inheritdoc />
        public HttpResponseMessage SendWorldChange(UpdatePlayerCurrentWorldRequest.PostData requestData)
        {
            var request = BuildWorldChangePostRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendWorldChangeAsync(UpdatePlayerCurrentWorldRequest.PostData requestData)
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
        public HttpResponseMessage SendAnnouncement(AnnouncementRequest.PostData requestData)
        {
            var request = BuildSendAnnouncementRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAnnouncementAsync(AnnouncementRequest.PostData requestData)
        {
            var request = BuildSendAnnouncementRequest(requestData);
            return this.httpClient.SendAsync(request);
        }

        /// <inheritdoc />
        public async void ConnectToPlayerEventStream()
        {
            try
            {
                // Avoid duplicate connections / connection attempts.
                if (this.PlayerEventStreamConnectionState is EventStreamConnectionState.Connecting or EventStreamConnectionState.Connected)
                {
                    throw new InvalidOperationException("Already connected or connecting to the player event stream.");
                }

                this.PlayerEventStreamConnectionState = EventStreamConnectionState.Connecting;

                // Begin reading the player event stream.
                using var stream = await this.httpClient.GetStreamAsync(PlayerEventsRequest.EndpointUrl);
                using var reader = new StreamReader(stream);
                Exception? exception = null;

                // Alert listeners that we are connected.
                this.PlayerEventStreamConnectionState = EventStreamConnectionState.Connected;
                this.OnPlayerStreamConnected?.Invoke(this);

                // Begin waiting for events.
                while (!reader.EndOfStream && this.PlayerEventStreamConnectionState == EventStreamConnectionState.Connected)
                {
                    var message = reader.ReadLine();
                    message = HttpUtility.UrlDecode(message);

                    // If the message was a heartbeat, alert listeners and continue.
                    if (message is null || message.Trim() == ":")
                    {
                        this.OnPlayerStreamHeartbeat?.Invoke(this);
                        continue;
                    }

                    // If the message contains empty data, skip it.
                    message = message.Replace("data:", "").Trim();
                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }

                    // Try to deserialize the message - if it fails due to invalid JSON, skip it, however if it fails for any other reason, break the loop.
                    try
                    {
                        var data = JsonSerializer.Deserialize<PlayerEventStreamUpdate>(message, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true });
                        this.OnPlayerStreamMessage?.Invoke(this, data);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        break;
                    }
                }

                // If the reader reaches the end of the stream without us intentionally disconnecting, alert listeners of the exception.
                if (reader.EndOfStream && this.PlayerEventStreamConnectionState == EventStreamConnectionState.Connected)
                {
                    this.OnPlayerStreamException?.Invoke(this, exception ?? new HttpRequestException("Connection to stream suddenly closed."));
                }
            }
            catch (Exception e)
            {
                // Anything else that goes wrong, alert listeners of the exception.
                this.OnPlayerStreamException?.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        public void DisconnectFromPlayerEventStream()
        {
            // Avoid duplicate disconnections / disconnection attempts.
            if (this.PlayerEventStreamConnectionState is EventStreamConnectionState.Disconnecting or EventStreamConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Already disconnected from the player event stream.");
            }

            // Cleanup and disconnect.
            this.PlayerEventStreamConnectionState = EventStreamConnectionState.Disconnecting;
            this.httpClient.CancelPendingRequests();

            // Alert listeners of the disconnect.
            this.PlayerEventStreamConnectionState = EventStreamConnectionState.Disconnected;
            this.OnPlayerStreamDisconnected?.Invoke(this);
        }

        /// <inheritdoc />
        public async void ConnectToAnnouncementsStream()
        {
            try
            {
                // Avoid duplicate connections / connection attempts.
                if (this.AnnouncementStreamConnectionState is EventStreamConnectionState.Connecting or EventStreamConnectionState.Connected)
                {
                    throw new InvalidOperationException("Already connected or connecting to the announcement stream.");
                }

                this.AnnouncementStreamConnectionState = EventStreamConnectionState.Connecting;

                // Begin reading the announcement stream.
                using var stream = await this.httpClient.GetStreamAsync(AnnouncementEventsRequest.EndpointUrl);
                using var reader = new StreamReader(stream);
                Exception? exception = null;

                // Alert listeners that we are connected.
                this.AnnouncementStreamConnectionState = EventStreamConnectionState.Connected;
                this.OnAnnouncementStreamConnected?.Invoke(this);

                // Begin waiting for events.
                while (!reader.EndOfStream && this.AnnouncementStreamConnectionState == EventStreamConnectionState.Connected)
                {
                    var message = reader.ReadLine();
                    message = HttpUtility.UrlDecode(message);

                    // If the message was a heartbeat, alert listeners and continue.
                    if (message is null || message.Trim() == ":")
                    {
                        this.OnAnnouncementStreamHeartbeat?.Invoke(this);
                        continue;
                    }

                    // If the message contains empty data, skip it.
                    message = message.Replace("data:", "").Trim();
                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }

                    // Try to deserialize the message - if it fails due to invalid JSON, skip it, however if it fails for any other reason, break the loop.
                    try
                    {
                        var data = JsonSerializer.Deserialize<AnnouncementStreamUpdate>(message, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true });
                        this.OnAnnouncementStreamMessage?.Invoke(this, data);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        break;
                    }
                }

                // If the reader reaches the end of the stream without us intentionally disconnecting, alert listeners of the exception.
                if (reader.EndOfStream && this.AnnouncementStreamConnectionState == EventStreamConnectionState.Connected)
                {
                    this.OnAnnouncementStreamException?.Invoke(this, exception ?? new HttpRequestException("Connection to stream suddenly closed."));
                }
            }
            catch (Exception e)
            {
                // Anything else that goes wrong, alert listeners of the exception.
                this.OnAnnouncementStreamException?.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        public void DisconnectFromAnnouncementsStream()
        {
            // Avoid duplicate disconnections / disconnection attempts.
            if (this.AnnouncementStreamConnectionState is EventStreamConnectionState.Disconnecting or EventStreamConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Already disconnected from the announcement stream.");
            }

            // Cleanup and disconnect.
            this.AnnouncementStreamConnectionState = EventStreamConnectionState.Disconnecting;
            this.httpClient.CancelPendingRequests();

            // Alert listeners of the disconnect.
            this.AnnouncementStreamConnectionState = EventStreamConnectionState.Disconnected;
            this.OnAnnouncementStreamDisconnected?.Invoke(this);
        }
    }

    /// <summary>
    ///     Represents the options for a <see cref="GoodfriendClient"/>.
    /// </summary>
    public readonly struct GoodfriendClientOptions
    {
        /// <summary>
        ///     The interval at which the client should attempt to reconnect to the player event stream after a connection loss.
        /// </summary>
        public readonly TimeSpan ReconnectInterval { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     The base address of the Goodfriend API.
        /// </summary>
        public readonly required Uri BaseAddress { get; init; }

        public GoodfriendClientOptions()
        {
        }
    }
}
