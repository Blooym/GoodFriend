using System;
using System.Collections.Generic;
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
        ///     The http client instance.
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        ///     The event stream reconnect timer instance.
        /// </summary>
        private readonly Timer eventStreamReconnectTimer;

        /// <summary>
        ///     Delegate for the <see cref="OnEventStreamPlayerStateUpdate" /> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="update">Player update data.</param>
        public delegate void DelegatePlayerStateUpdate(object? sender, EventStreamPlayerStateUpdateResponse update);

        /// <summary>
        ///     The event for when a player state update is recieved.
        /// </summary>
        public event DelegatePlayerStateUpdate? OnEventStreamPlayerStateUpdate;

        /// <summary>
        ///     Delegate for the <see cref="OnEventStreamException" /> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        public delegate void DelegateEventStreamError(object? sender, Exception exception);

        /// <summary>
        ///     The event for when an exception relating to the event stream connection is recieved.
        /// </summary>
        /// <remarks>
        ///     Automatic reconnection is handled by the client and does not need to be implemented
        ///     by listening for this event.
        /// </remarks>
        public event DelegateEventStreamError? OnEventStreamException;

        /// <summary>
        ///     Delegate for the <see cref="OnEventStreamConnected" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegateEventStreamConnected(object? sender);

        /// <summary>
        ///     The event for when the event stream has finished connecting successfully.
        /// </summary>
        public event DelegateEventStreamConnected? OnEventStreamConnected;

        /// <summary>
        ///     Delegate for the <see cref="OnEventStreamHeartbeat" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegateEventStreamHeartbeat(object? sender);

        /// <summary>
        ///     The event for when a heartbeat is recieved from the event stream.
        /// </summary>
        public event DelegateEventStreamHeartbeat? OnEventStreamHeartbeat;

        /// <summary>
        ///     Delegate for the <see cref="OnEventStreamConnected" /> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void DelegateEventStreamDisconnected(object? sender);

        /// <summary>
        ///     The event for when the event stream has disconnected.
        /// </summary>
        /// <remarks>
        ///     Not fired when an exception is handled, only when the client disconnected properly.
        /// </remarks>
        public event DelegateEventStreamDisconnected? OnEventStreamDisconnected;

        /// <inheritdoc />
        public EventStreamConnectionState ConnectionState { get; private set; } = EventStreamConnectionState.Disconnected;

        /// <inheritdoc />
        public GoodfriendClientOptions Options { get; private set; }

        /// <inheritdoc />
        public string GameVersion { get; }

        /// <summary>
        ///     Creates a new instance of the client.
        /// </summary>
        /// <param name="baseUri">The base uri of the api</param>
        /// <param name="gameVersion">The current ffxivgame.ver value</param>
        public GoodFriendClient(GoodfriendClientOptions options, string gameVersion)
        {
            this.Options = options;
            this.GameVersion = gameVersion;
            this.httpClientHandler = CreateHttpClientHandler();
            this.httpClient = CreateHttpClient(this.httpClientHandler, options.BaseAddress, gameVersion);
            this.eventStreamReconnectTimer = new Timer(options.ReconnectInterval);
            this.eventStreamReconnectTimer.Elapsed += this.HandleReconnectTimerElapse;
            this.OnEventStreamException += this.HandleEventStreamException;
            this.OnEventStreamConnected += this.HandleEventStreamConnected;
            this.OnEventStreamDisconnected += this.HandleEventStreamDisconnected;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                this.DisconnectFromEventStream();
            }
            catch
            {
                //
            }

            this.httpClient.CancelPendingRequests();
            this.httpClient.Dispose();
            this.httpClientHandler.Dispose();

            this.eventStreamReconnectTimer.Stop();
            this.eventStreamReconnectTimer.Dispose();
            this.eventStreamReconnectTimer.Elapsed -= this.HandleReconnectTimerElapse;

            this.OnEventStreamException -= this.HandleEventStreamException;
            this.OnEventStreamConnected -= this.HandleEventStreamConnected;
            this.OnEventStreamDisconnected -= this.HandleEventStreamDisconnected;
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
        private static HttpClient CreateHttpClient(HttpClientHandler handler, Uri baseUri, string gameVersion) => new(handler)
        {
            BaseAddress = baseUri,
            DefaultRequestHeaders =
            {
                { "User-Agent", UserAgent },
                { "X-Game-Version", gameVersion },
            },
            Timeout = TimeSpan.FromSeconds(10),
        };

        /// <summary>
        ///     Creates a query string from the given <paramref name="parameters" />
        /// </summary>
        /// <param name="parameters">The parameters to create the query string from</param>
        /// <returns>A ready to use query string</returns>
        private static string CreateParams(Dictionary<string, object> parameters)
        {
            var sb = new StringBuilder("?");
            foreach (var (key, value) in parameters)
            {
                sb.Append($"{key}={value}&");
            }
            return sb.ToString().TrimEnd('&');
        }

        /// <summary>
        ///     When the event stream reconnect timer elapses, attempt to reconnect to the event stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleReconnectTimerElapse(object? sender, ElapsedEventArgs e)
        {
            // If we are not in an exception state, stop the timer and return.
            if (this.ConnectionState is not EventStreamConnectionState.Exception)
            {
                this.eventStreamReconnectTimer.Stop();
                return;
            }

            // Attempt to reconnect to the event stream.
            this.ConnectToEventStream();
        }

        /// <summary>
        ///     Handles the event stream exception event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void HandleEventStreamException(object? sender, Exception exception)
        {
            this.ConnectionState = EventStreamConnectionState.Exception;
            this.httpClient.CancelPendingRequests();
            this.eventStreamReconnectTimer.Start();
        }

        /// <summary>
        ///     Handles the event stream connected event.
        /// </summary>
        /// <param name="sender"></param>
        private void HandleEventStreamConnected(object? sender) => this.eventStreamReconnectTimer.Stop();

        /// <summary>
        ///     Handles the event stream disconnected event.
        /// </summary>
        /// <param name="sender"></param>
        private void HandleEventStreamDisconnected(object? sender) => this.eventStreamReconnectTimer.Stop();

        /// <summary>
        ///     Builds a new login state update request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildLoginStatePutRequest(UpdatePlayerLoginStateRequest.PutData requestData)
        {
            var queryParams = CreateParams(new Dictionary<string, object>
            {
                { UpdatePlayerLoginStateRequest.PutData.CONTENT_ID_PARAM, requestData.ContentIdHash },
                { UpdatePlayerLoginStateRequest.PutData.CONTENT_ID_SALT_PARAM, requestData.ContentIdSalt },
                { UpdatePlayerLoginStateRequest.PutData.DATACENTER_ID_PARAM, requestData.WorldId },
                { UpdatePlayerLoginStateRequest.PutData.WORLD_ID_PARAM, requestData.WorldId },
                { UpdatePlayerLoginStateRequest.PutData.TERRITORY_ID_PARAM, requestData.TerritoryId },
                { UpdatePlayerLoginStateRequest.PutData.LOGGED_IN_PARAM, requestData.LoggedIn },
            });
            return new HttpRequestMessage(HttpMethod.Put, UpdatePlayerLoginStateRequest.ENDPOINT_URL + queryParams);
        }

        /// <summary>
        ///     Builds a new world change request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildWorldChangePutRequest(UpdatePlayerWorldRequest.PutData requestData)
        {
            var queryParams = CreateParams(new Dictionary<string, object>
            {
                { UpdatePlayerWorldRequest.PutData.CONTENT_ID_PARAM, requestData.ContentIdHash },
                { UpdatePlayerWorldRequest.PutData.CONTENT_ID_SALT_PARAM, requestData.ContentIdSalt },
                { UpdatePlayerWorldRequest.PutData.WORLD_ID_PARAM, requestData.WorldId },
            });
            return new HttpRequestMessage(HttpMethod.Put, UpdatePlayerWorldRequest.ENDPOINT_URL + queryParams);
        }

        private static HttpRequestMessage BuildMinimumVersionRequest() => new(HttpMethod.Get, MinimumGameVersionRequest.ENDPOINT_URL);

        /// <summary>
        ///     Builds a new metadata request.
        /// </summary>
        /// <returns></returns>
        private static HttpRequestMessage BuildMetadataRequest() => new(HttpMethod.Get, MetadataRequest.ENDPOINT_URL);

        /// <inheritdoc />
        public HttpResponseMessage SendLoginState(UpdatePlayerLoginStateRequest.PutData requestData)
        {
            var request = BuildLoginStatePutRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendLoginStateAsync(UpdatePlayerLoginStateRequest.PutData requestData)
        {
            var request = BuildLoginStatePutRequest(requestData);
            return this.httpClient.SendAsync(request);
        }

        /// <inheritdoc />
        public HttpResponseMessage SendWorldChange(UpdatePlayerWorldRequest.PutData requestData)
        {
            var request = BuildWorldChangePutRequest(requestData);
            return this.httpClient.Send(request);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendWorldChangeAsync(UpdatePlayerWorldRequest.PutData requestData)
        {
            var request = BuildWorldChangePutRequest(requestData);
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
        public (MinimumGameVersionResponse, HttpResponseMessage) GetMinimumVersion()
        {
            var request = BuildMinimumVersionRequest();
            var response = this.httpClient.Send(request);
            return (response.Content.ReadFromJsonAsync<MinimumGameVersionResponse>().Result, response);
        }

        /// <inheritdoc />
        public async Task<(MinimumGameVersionResponse, HttpResponseMessage)> GetMinimumVersionAsync()
        {
            var request = BuildMinimumVersionRequest();
            var response = await this.httpClient.SendAsync(request);
            return (await response.Content.ReadFromJsonAsync<MinimumGameVersionResponse>(), response);
        }

        /// <inheritdoc />
        public async void ConnectToEventStream()
        {
            try
            {
                // Avoid duplicate connections / connection attempts.
                if (this.ConnectionState is EventStreamConnectionState.Connecting or EventStreamConnectionState.Connected)
                {
                    throw new InvalidOperationException("Already connected or connecting to the event stream.");
                }

                this.ConnectionState = EventStreamConnectionState.Connecting;

                // Begin reading the event stream.
                using var request = new HttpRequestMessage(HttpMethod.Get, PlayerEventsRequest.ENDPOINT_URL);
                using var stream = await this.httpClient.GetStreamAsync(PlayerEventsRequest.ENDPOINT_URL);
                using var reader = new StreamReader(stream);
                Exception? exception = null;

                // Alert listeners that we are connected.
                this.ConnectionState = EventStreamConnectionState.Connected;
                this.OnEventStreamConnected?.Invoke(this);

                // Begin waiting for events.
                while (!reader.EndOfStream && this.ConnectionState == EventStreamConnectionState.Connected)
                {
                    var message = reader.ReadLine();
                    message = HttpUtility.UrlDecode(message);

                    // If the message was a heartbeat, alert listeners and continue.
                    if (message == null || message.Trim() == ":")
                    {
                        this.OnEventStreamHeartbeat?.Invoke(this);
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
                        var data = JsonSerializer.Deserialize<EventStreamPlayerStateUpdateResponse>(message, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true });
                        this.OnEventStreamPlayerStateUpdate?.Invoke(this, data);
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
                if (reader.EndOfStream && this.ConnectionState == EventStreamConnectionState.Connected)
                {
                    this.OnEventStreamException?.Invoke(this, exception ?? new HttpRequestException("Connection to stream suddenly closed."));
                }
            }
            catch (Exception e)
            {
                // Anything else that goes wrong, alert listeners of the exception.
                this.OnEventStreamException?.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        public void DisconnectFromEventStream()
        {
            // Avoid duplicate disconnections / disconnection attempts.
            if (this.ConnectionState is EventStreamConnectionState.Disconnecting or EventStreamConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Already disconnected from the event stream.");
            }

            // Cleanup and disconnect.
            this.ConnectionState = EventStreamConnectionState.Disconnecting;
            this.httpClient.CancelPendingRequests();

            // Alert listeners of the disconnect.
            this.ConnectionState = EventStreamConnectionState.Disconnected;
            this.OnEventStreamDisconnected?.Invoke(this);
        }

        ///<inheritdoc/>
        public void UpdateOptions(GoodfriendClientOptions options)
        {
            if (options.BaseAddress != this.Options.BaseAddress)
            {
                var wasConnected = this.ConnectionState == EventStreamConnectionState.Connected;
                if (wasConnected)
                {
                    this.DisconnectFromEventStream();
                }

                this.httpClient.Dispose();
                this.httpClient = CreateHttpClient(this.httpClientHandler, options.BaseAddress, this.GameVersion);

                if (wasConnected)
                {
                    this.ConnectToEventStream();
                }
            }

            if (options.ReconnectInterval != this.Options.ReconnectInterval)
            {
                this.eventStreamReconnectTimer.Interval = options.ReconnectInterval.TotalMilliseconds;
            }

            this.Options = options;
        }
    }

    /// <summary>
    ///     Represents the options for a <see cref="GoodfriendClient"/>.
    /// </summary>
    public readonly struct GoodfriendClientOptions
    {
        /// <summary>
        ///     The interval at which the client should attempt to reconnect to the event stream after a connection loss.
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