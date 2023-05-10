using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using GoodFriend.Client.Requests;
using GoodFriend.Client.Responses;
using RestSharp;

namespace GoodFriend.Client
{
    /// <summary>
    ///     A GoodFriend API-compatiable client.
    /// </summary>
    public sealed class GoodFriendClient : IGoodFriendClient
    {
        /// <summary>
        ///     How often the event stream should attempt to reconnect if it is disconnected.
        /// </summary>
        private const double AUTO_RECONNECT_SECONDS = 45;

        /// <summary>
        ///     The rest client instance.
        /// </summary>
        private readonly RestClient restClient;

        /// <summary>
        ///     The http client instance.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        ///     The event stream reconnect timer instance.
        /// </summary>
        private readonly Timer eventStreamReconnectTimer = new(TimeSpan.FromSeconds(AUTO_RECONNECT_SECONDS));

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
        public Uri BaseUri { get; }

        /// <summary>
        ///     Creates a new instance of the client.
        /// </summary>
        /// <param name="baseUri">The base uri of the api</param>
        /// <param name="subPath">The sub path of the api</param>
        public GoodFriendClient(Uri baseUri)
        {
            this.BaseUri = baseUri;
            this.restClient = CreateRestClient(baseUri);
            this.httpClient = CreateHttpClient(baseUri);
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

            this.restClient.Dispose();
            this.httpClient.Dispose();

            this.eventStreamReconnectTimer.Stop();
            this.eventStreamReconnectTimer.Dispose();
            this.eventStreamReconnectTimer.Elapsed -= this.HandleReconnectTimerElapse;

            this.OnEventStreamException -= this.HandleEventStreamException;
            this.OnEventStreamConnected -= this.HandleEventStreamConnected;
            this.OnEventStreamDisconnected -= this.HandleEventStreamDisconnected;
        }

        /// <summary>
        ///     Creates a new custom <see cref="RestClient" /> with the given <paramref name="baseUri" />.
        /// </summary>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        private static RestClient CreateRestClient(Uri baseUri) => new(new RestClientOptions()
        {
            BaseUrl = baseUri,
        });

        /// <summary>
        ///     Creates a new custom <see cref="HttpClient" /> with the given <paramref name="baseUri" />
        /// </summary>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        private static HttpClient CreateHttpClient(Uri baseUri) => new()
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(10),
        };

        /// <summary>
        ///     Builds a new login state update request with the given <paramref name="requestData" />
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static RestRequest BuildLoginStateRequest(UpdatePlayerLoginStateRequest.PutData requestData)
        {
            var request = new RestRequest(UpdatePlayerLoginStateRequest.EndpointUrl);
            request.AddQueryParameter(UpdatePlayerLoginStateRequest.PutData.ContentIdParam, requestData.ContentIdHash);
            request.AddQueryParameter(UpdatePlayerLoginStateRequest.PutData.ContentIdSaltParam, requestData.ContentIdSalt);
            request.AddQueryParameter(UpdatePlayerLoginStateRequest.PutData.DatacenterIdParam, requestData.DatacenterId);
            request.AddQueryParameter(UpdatePlayerLoginStateRequest.PutData.WorldIdParam, requestData.WorldId);
            request.AddQueryParameter(UpdatePlayerLoginStateRequest.PutData.TerritoryIdParam, requestData.TerritoryId);
            request.AddQueryParameter(UpdatePlayerLoginStateRequest.PutData.LoggedInParam, requestData.LoggedIn);
            return request;
        }

        /// <inheritdoc />
        public RestResponse SendLoginState(UpdatePlayerLoginStateRequest.PutData requestData)
        {
            var request = BuildLoginStateRequest(requestData);
            return this.restClient.Put(request);
        }

        /// <inheritdoc />
        public Task<RestResponse> SendLoginStateAsync(UpdatePlayerLoginStateRequest.PutData requestData)
        {
            var request = BuildLoginStateRequest(requestData);
            return this.restClient.PutAsync(request);
        }

        /// <summary>
        ///     Builds a new metadata request.
        /// </summary>
        /// <returns></returns>
        private static RestRequest BuildMetadataRequest() => new(MetadataRequest.EndpointUrl);

        /// <inheritdoc />
        public MetadataResponse GetMetadata()
        {
            var request = BuildMetadataRequest();
            return this.restClient.Get<MetadataResponse>(request);
        }

        /// <inheritdoc />
        public Task<MetadataResponse> GetMetadataAsync()
        {
            var request = BuildMetadataRequest();
            return this.restClient.GetAsync<MetadataResponse>(request);
        }

        private static RestRequest BuildMotdRequest() => new(MotdRequest.EndpointUrl);

        /// <inheritdoc />
        public MotdResponse GetMotd()
        {
            var request = BuildMotdRequest();
            return this.restClient.Get<MotdResponse>(request);
        }

        /// <inheritdoc />
        public Task<MotdResponse> GetMotdAsync()
        {
            var request = BuildMotdRequest();
            return this.restClient.GetAsync<MotdResponse>(request);
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
                using var stream = await this.httpClient.GetStreamAsync(PlayerEventsRequest.EndpointUrl);
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
    }
}