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
    // TODO: Implement server-sent events client
    /// <summary>
    ///     A GoodFriend API-compatiable client.
    /// </summary>
    public sealed class GoodFriendClient : IGoodFriendClient, IDisposable
    {
        /// <inheritdoc />
        public Uri BaseUri { get; }

        /// <inheritdoc />
        public EventStreamConnectionState ConnectionState { get; private set; } = EventStreamConnectionState.Disconnected;

        private readonly RestClient restClient;
        private readonly HttpClient httpClient;
        private readonly Timer eventStreamReconnectTimer;

        public delegate void DelegatePlayerStateUpdate(object? sender, EventStreamPlayerUpdate update);
        public event DelegatePlayerStateUpdate? OnEventStreamStateUpdate;

        public delegate void DelegateEventStreamError(object? sender, Exception exception);
        public event DelegateEventStreamError? OnEventStreamException;

        public delegate void DelegateEventStreamConnected(object? sender);
        public event DelegateEventStreamConnected? OnEventStreamConnected;

        public delegate void DelegateEventStreamHeartbeat(object? sender);
        public event DelegateEventStreamHeartbeat? OnEventStreamHeartbeat;

        public delegate void DelegateEventStreamDisconnected(object? sender);
        public event DelegateEventStreamDisconnected? OnEventStreamDisconnected;

        public GoodFriendClient(Uri baseUri)
        {
            this.BaseUri = baseUri;
            this.restClient = new RestClient(new RestClientOptions()
            {
                BaseUrl = this.BaseUri,
            });
            this.httpClient = new HttpClient()
            {
                BaseAddress = this.BaseUri,
            };
            this.eventStreamReconnectTimer = new(60000);
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

        private static RestRequest BuildLoginRequest(LoginRequest requestData)
        {
            var request = new RestRequest(LoginRequest.EndpointUrl);
            request.AddQueryParameter(LoginRequest.ContentIdParam, requestData.ContentIdHash);
            request.AddQueryParameter(LoginRequest.ContentIdSaltParam, requestData.ContentIdSalt);
            request.AddQueryParameter(LoginRequest.DatacenterIdParam, requestData.DatacenterId);
            request.AddQueryParameter(LoginRequest.WorldIdParam, requestData.WorldId);
            request.AddQueryParameter(LoginRequest.TerritoryIdParam, requestData.TerritoryId);
            return request;
        }

        /// <inheritdoc />
        public RestResponse SendLogin(LoginRequest requestData)
        {
            var request = BuildLoginRequest(requestData);
            return this.restClient.Put(request);
        }

        /// <inheritdoc />
        public Task<RestResponse> SendLoginAsync(LoginRequest requestData)
        {
            var request = BuildLoginRequest(requestData);
            return this.restClient.PutAsync(request);
        }

        private static RestRequest BuildLogoutRequest(LogoutRequest requestData)
        {
            var request = new RestRequest(LogoutRequest.EndpointUrl);
            request.AddQueryParameter(LogoutRequest.ContentIdParam, requestData.ContentIdHash);
            request.AddQueryParameter(LogoutRequest.ContentIdSaltParam, requestData.ContentIdSalt);
            request.AddQueryParameter(LogoutRequest.DatacenterIdParam, requestData.DatacenterId);
            request.AddQueryParameter(LogoutRequest.WorldIdParam, requestData.WorldId);
            request.AddQueryParameter(LogoutRequest.TerritoryIdParam, requestData.TerritoryId);
            return request;
        }

        /// <inheritdoc />
        public RestResponse SendLogout(LogoutRequest requestData)
        {
            var request = BuildLogoutRequest(requestData);
            return this.restClient.Put(request);
        }

        /// <inheritdoc />
        public Task<RestResponse> SendLogoutAsync(LogoutRequest requestData)
        {
            var request = BuildLogoutRequest(requestData);
            return this.restClient.PutAsync(request);
        }

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
                if (this.ConnectionState is EventStreamConnectionState.Connecting or EventStreamConnectionState.Connected)
                {
                    throw new InvalidOperationException("Already connected or connecting to the event stream");
                }

                this.ConnectionState = EventStreamConnectionState.Connecting;

                using var stream = await this.httpClient.GetStreamAsync(EventsRequest.EndpointUrl);
                using var reader = new StreamReader(stream);

                this.ConnectionState = EventStreamConnectionState.Connected;
                this.OnEventStreamConnected?.Invoke(this);

                while (!reader.EndOfStream && this.ConnectionState == EventStreamConnectionState.Connected)
                {
                    var message = reader.ReadLine();
                    message = HttpUtility.UrlDecode(message);

                    if (message == null || message.Trim() == ":")
                    {
                        this.OnEventStreamHeartbeat?.Invoke(this);
                        continue;
                    }

                    message = message.Replace("data:", "").Trim();

                    try
                    {
                        var data = JsonSerializer.Deserialize<EventStreamPlayerUpdate>(message);
                        this.OnEventStreamStateUpdate?.Invoke(this, data);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (reader.EndOfStream && this.ConnectionState == EventStreamConnectionState.Connected)
                {
                    this.OnEventStreamException?.Invoke(this, new HttpRequestException("Stream suddenly closed"));
                }
            }
            catch (Exception e)
            {
                this.OnEventStreamException?.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        public void DisconnectFromEventStream()
        {
            if (this.ConnectionState is EventStreamConnectionState.Disconnecting or EventStreamConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Already disconnected from the event stream.");
            }

            this.ConnectionState = EventStreamConnectionState.Disconnecting;
            this.httpClient.CancelPendingRequests();
            this.ConnectionState = EventStreamConnectionState.Disconnected;
            this.OnEventStreamDisconnected?.Invoke(this);
        }

        private void HandleReconnectTimerElapse(object? sender, ElapsedEventArgs e)
        {
            if (this.ConnectionState is not EventStreamConnectionState.Exception)
            {
                this.eventStreamReconnectTimer.Stop();
                return;
            }

            this.ConnectToEventStream();
        }

        private void HandleEventStreamException(object? sender, Exception exception)
        {
            this.ConnectionState = EventStreamConnectionState.Exception;
            this.eventStreamReconnectTimer.Start();
        }

        private void HandleEventStreamConnected(object? sender) => this.eventStreamReconnectTimer.Stop();
        private void HandleEventStreamDisconnected(object? sender) => this.eventStreamReconnectTimer.Stop();
    }
}