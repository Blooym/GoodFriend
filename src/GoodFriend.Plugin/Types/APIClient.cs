namespace GoodFriend.Types
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Collections.Generic;
    using GoodFriend.Base;
    using GoodFriend.Utils;
    using Dalamud.Logging;
    using Newtonsoft.Json;

    /// <summary>
    ///     An API client to built to communicate with the GoodFriend API.
    /// </summary>
    public sealed class APIClient : IDisposable
    {
        /// <summary>
        ///     The Delegate that is used for <see cref="APIClient.SSEDataReceived"/>.
        /// </summary>
        public delegate void SSEDataReceivedDelegate(UpdatePayload data);

        /// <summary>
        ///     Fired when data is new SSE data is received.
        /// </summary>
        public event SSEDataReceivedDelegate? SSEDataReceived;

        /// <summary>
        ///     The Delegate that is used for <see cref="APIClient.SSEConnectionEstablished"/>.
        /// </summary>
        public delegate void SSEConnectionEstablishedDelegate();

        /// <summary>
        ///     Fired when a successful SSE connection is established.
        /// </summary>
        public event SSEConnectionEstablishedDelegate? SSEConnectionEstablished;

        /// <summary>
        //      The Delegate that is used for <see cref="APIClient.SSEConnectionClosed"/>.
        /// </summary> 
        public delegate void SSEConnectionClosedDelegate();

        /// <summary>
        ///     Fired when the SSE connection is closed.
        /// </summary>
        public event SSEConnectionClosedDelegate? SSEConnectionClosed;

        /// <summary> 
        ///     The Delegate that is used for <see cref="APIClient.SSEConnectionError"/>.
        /// </summary>
        public delegate void SSEConnectionErrorDelegate(Exception error);

        /// <summary>
        ///     Fired when an error occurs with the SSE connection.
        /// </summary>
        public event SSEConnectionErrorDelegate? SSEConnectionError;

        /// <summary>
        ///     The Delegate that is used for <see cref="APIClient.RequestError"/>.
        /// </summary>
        public delegate void RequestErrorDelegate(Exception error, HttpResponseMessage? response);

        /// <summary>
        ///     Fired when an error occurs with a request, use <see cref="APIClient.SSEConnectionError"/> for SSE errors.
        /// </summary>
        public event RequestErrorDelegate? RequestError;

        /// <summary>
        ///     The Delegate that is used for <see cref="APIClient.RequestSuccess"/>.
        /// </summary>
        public delegate void RequestSuccessDelegate(HttpResponseMessage response);

        /// <summary>
        ///     Fired when a request is successful.
        /// </summary>
        public event RequestSuccessDelegate? RequestSuccess;

        /// <summary>
        ///     Handles a successful connection to the API.
        /// </summary>
        private void OnSSEConnectionEstablished()
        {
            this.SSEIsConnected = true;
            this.SSEIsConnecting = false;
            this.LastStatusCode = HttpStatusCode.OK;
            this._httpClient?.CancelPendingRequests();
            this._sseReconnectTimer.Stop();
        }

        /// <summary>
        ///     Handles a connection closure (non-error).
        /// </summary>
        private void OnSSEConnectionClosed()
        {
            this.SSEIsConnected = false;
            this.SSEIsConnecting = false;
            this._httpClient.CancelPendingRequests();
            this._sseReconnectTimer.Stop();
        }

        /// <summary>
        ///     Handles errors with the SSE connection.
        /// </summary>
        private void OnSSEConnectionError(Exception error)
        {
            this.SSEIsConnected = false;
            this.SSEIsConnecting = false;
            this._httpClient.CancelPendingRequests();
            this._sseReconnectTimer.Start();
        }

        /// <summary> 
        ///     The configuration for the API client.
        /// </summary>
        private readonly Configuration _configuration;

        /// <summary>
        ///     The API version this client is compatible with, ending with a slash.
        /// </summary>
        public readonly string apiVersion = "v4";

        /// <summary>
        ///     The base URL for the API with the appended API version.
        /// </summary>
        private Uri _apiBaseURL => new($"{this._configuration.APIUrl}{this.apiVersion}/");

        /// <summary>
        ///     The HTTP client used to connect to the API.
        /// </summary>
        private HttpClient _httpClient = new HttpClient();

        /// <summary>
        ///     Configures the HTTP client.
        /// </summary>
        private void ConfigureHttpClient()
        {
            this._httpClient.BaseAddress = this._apiBaseURL;
            this._httpClient.Timeout = TimeSpan.FromSeconds(10);

            // Headers
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", $"Dalamud.{Common.RemoveWhitespace(PStrings.pluginName)}/{Assembly.GetExecutingAssembly().GetName().Version}");
            this._httpClient.DefaultRequestHeaders.Add("X-Session-Identifier", Guid.NewGuid().ToString());
            if (this._configuration.APIAuthentication != string.Empty) this._httpClient.DefaultRequestHeaders.Add("Authorization", $"{this._configuration.APIAuthentication}");

            var headers = this._httpClient.DefaultRequestHeaders.ToString().Replace($"Authorization: {this._configuration.APIAuthentication}", "Authorization: [REDACTED]");
            PluginLog.Information($"APIClient(ConfigureHttpClient): Successfully configured HTTP client.\nBaseAddress: {this._httpClient.BaseAddress}\n{headers}\nTimeout: {this._httpClient.Timeout}");
        }

        /// <summary>
        ///    The current state of the SSE connection.
        /// </summary>
        public bool SSEIsConnected { get; private set; } = false;

        /// <summary>
        ///     If the APIClient is currently attempting an SSE connection.
        /// </summary>
        public bool SSEIsConnecting { get; private set; } = false;

        /// <summary>
        ///     The timer for handling SSE reconnection attempts.
        /// </summary>
        private System.Timers.Timer _sseReconnectTimer = new System.Timers.Timer(60000);

        /// <summary>
        ///     The event handler for handling SSE reconnection attempts.
        /// </summary>
        private void OnTryReconnect(object? sender, ElapsedEventArgs e) => this.OpenSSEStream();

        /// <summary>
        ///     A timestamp in seconds for when the client will no longer be rate limited.
        /// </summary>
        public DateTime RateLimitReset { get; private set; } = DateTime.Now;

        /// <summary>
        ///     Handles ratelimits from the API and sets the LastRequestRateLimited and RateLimitReset properties.
        /// </summary>
        private void HandleRatelimitAndStatuscode(HttpResponseMessage response)
        {
            if (response.StatusCode == (HttpStatusCode)429)
            {
                PluginLog.Warning($"APIClient(HandleRatelimitAndStatuscode): Ratelimited by the API, adjusting requests accordingly.");
                this.LastStatusCode = response.StatusCode;

                if (response.Headers.TryGetValues("ratelimit-reset", out IEnumerable<string>? values))
                {
                    this.RateLimitReset = DateTime.Now.AddSeconds(int.Parse(values.First()));
                    PluginLog.Information($"APIClient(HandleRatelimitAndStatuscode): received ratelimit-reset header of {values.First()} seconds, setting reset time to {this.RateLimitReset}");
                }
                else
                {
                    PluginLog.Information($"APIClient(HandleRatelimitAndStatuscode): No ratelimit-reset header received, setting reset time to a fallback value.");
                    this.RateLimitReset = response.Headers.TryGetValues("retry-after", out IEnumerable<string>? retryValues)
                        ? DateTime.Now.AddSeconds(int.Parse(retryValues.First()))
                        : DateTime.Now.AddSeconds(60);
                }
            }
            else
            {
                this.LastStatusCode = response.StatusCode;
            }
        }

        /// <summary>
        ///     Cancels all pending HTTP requests. This is used to cancel SSE connections.
        /// </summary>
        public void CancelPendingRequests() { this._httpClient.CancelPendingRequests(); this._sseReconnectTimer.Stop(); this.SSEIsConnecting = false; }

        /// <summary>
        ///     The last status code received from the API.
        /// </summary>
        public HttpStatusCode LastStatusCode { get; private set; } = 0;

        /// <summary>
        ///     Instantiates a new APIClient
        /// </summary>
        public APIClient(Configuration config)
        {
            this._configuration = config;

            this.SSEConnectionEstablished += OnSSEConnectionEstablished;
            this.SSEConnectionClosed += OnSSEConnectionClosed;
            this.SSEConnectionError += OnSSEConnectionError;
            this._sseReconnectTimer.Elapsed += OnTryReconnect;
            this.ConfigureHttpClient();

            PluginLog.Debug($"APIClient(APIClient): Instantiated.");
        }

        /// <summary>
        ///     Disposes of the APIClient and its resources.
        /// </summary>
        public void Dispose()
        {
            // If we're still connected, disconecct.
            if (this.SSEIsConnected) this.CloseSSEStream();

            // Unregister any event handlers.
            this.SSEConnectionEstablished -= OnSSEConnectionEstablished;
            this.SSEConnectionClosed -= OnSSEConnectionClosed;
            this.SSEConnectionError -= OnSSEConnectionError;
            this._sseReconnectTimer.Elapsed -= OnTryReconnect;

            // Cancel pending requests.
            this._httpClient.CancelPendingRequests();

            // Dispose of all other resources.
            this._sseReconnectTimer.Dispose();
            this._httpClient.Dispose();

            PluginLog.Debug("APIClient(Dispose): Successfully disposed.");
        }

        /// <summary> 
        ///     Opens the connection stream to the API, throws error if already connected.
        /// </summary>
        public void OpenSSEStream()
        {
            if (SSEIsConnected) throw new InvalidOperationException("An active connection has already been established.");
            this.OpenSSEStreamConnection();
        }

        /// <summary>
        //      Closes the connection stream to the API, throws error if not connected.
        /// </summary>
        public void CloseSSEStream()
        {
            if (!SSEIsConnected) throw new InvalidOperationException("There is no active connection to disconnect from.");
            this.SSEConnectionClosed?.Invoke();
        }

        /// <summary>
        ///     Starts listening for data from the API.
        /// </summary>
        private async void OpenSSEStreamConnection()
        {
            try
            {
                // If we're already connecting, don't try again.
                if (this.SSEIsConnecting) return;
                this.SSEIsConnecting = true;

                // Check to see if we're ratelimited by sending a request to the root endpoint.
                HttpResponseMessage response = await this._httpClient.GetAsync(this._httpClient.BaseAddress);
                this.HandleRatelimitAndStatuscode(response);

                // If we are ratelimited, hold off until we're not.
                if (this.RateLimitReset > DateTime.Now)
                {
                    PluginLog.Information($"APIClient(OpenSSEStreamConnection): Not connecting until rate limit is reset at {this.RateLimitReset}");
                    await Task.Run(() => { while (this.RateLimitReset > DateTime.Now) Thread.Sleep(1000); });
                }

                // Attempt to connect to the API.
                PluginLog.Information($"APIClient(OpenSSEStreamConnection): Connecting to {this._httpClient.BaseAddress}sse/friends");
                using var stream = await this._httpClient.GetStreamAsync($"{this._httpClient.BaseAddress}sse/friends");
                using var reader = new StreamReader(stream);

                // Connection established! Start listening for data.
                this.SSEConnectionEstablished?.Invoke();

                while (!reader.EndOfStream && SSEIsConnected)
                {
                    // Read the message, if its null or just a colon (:) then skip it. (Colon indicates a heartbeat message).
                    var message = reader.ReadLine();
                    message = HttpUtility.UrlDecode(message);
                    if (message == null || message.Trim() == ":") continue;

                    // Remove any SSE (Server-Sent Events) filler & parse the message.
                    message = message.Replace("data: ", "").Trim();

                    var data = JsonConvert.DeserializeObject<UpdatePayload>(message);
                    if (data != null && data.ContentID != null) SSEDataReceived?.Invoke(data);
                }

                if (reader.EndOfStream && SSEIsConnected)
                {
                    this.CloseSSEStream();
                }
            }
            catch (Exception e)
            {
                if (SSEIsConnected) this.CloseSSEStream();
                this.SSEConnectionError?.Invoke(e);
            }
        }

        /// <summary>
        ///     Fetches the latest metadata from the API.
        /// </summary>
        public MetadataPayload? GetMetadata()
        {
            if (this.RateLimitReset > DateTime.Now)
            {
                this.RequestError?.Invoke(new Exception($"Ratelimited. Try again at {this.RateLimitReset}"), new HttpResponseMessage(HttpStatusCode.TooManyRequests));
                return null;
            }

            using var requestData = new HttpRequestMessage
            (
                HttpMethod.Get,
                "metadata"
            );

            try
            {
                var response = this._httpClient.SendAsync(requestData).Result;
                if (!response.IsSuccessStatusCode)
                {
                    this.RequestError?.Invoke(new Exception($"Request failed with status code {response.StatusCode}"), response);
                    return null;
                }
                else
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<MetadataPayload>(responseContent);
                    this.RequestSuccess?.Invoke(response);
                    return data;
                }
            }
            catch (Exception e)
            {
                this.RequestError?.Invoke(e, null);
                return null;
            }
        }

        /// <summary>
        ///     Send a login event to the configured API/logout endpoint.
        /// </summary>
        public void SendLogin(ulong contentID, uint homeworldID, uint territoryID)
        {
            using var request = new HttpRequestMessage
            (
                HttpMethod.Put,
                $"login?contentID={HttpUtility.UrlEncode(Hashing.HashSHA512(contentID.ToString()))}&homeworldID={homeworldID}&territoryID={territoryID}"
            );

            try
            {
                var result = this._httpClient.SendAsync(request).Result;
                if (!result.IsSuccessStatusCode)
                    this.RequestError?.Invoke(new Exception($"Request failed with status code {result.StatusCode}"), result);
                else
                    this.RequestSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                this.RequestError?.Invoke(e, null);
            }
        }

        /// <summary>
        ///     Send a logout event to the configured API/logout endpoint.
        /// </summary>
        public void SendLogout(ulong contentID, uint homeworldID, uint territoryID)
        {
            using var request = new HttpRequestMessage
            (
                HttpMethod.Put,
                $"logout?contentID={HttpUtility.UrlEncode(Hashing.HashSHA512(contentID.ToString()))}&homeworldID={homeworldID}&territoryID={territoryID}"
            );

            try
            {
                var result = this._httpClient.SendAsync(request).Result;
                if (!result.IsSuccessStatusCode)
                    this.RequestError?.Invoke(new Exception($"Request failed with status code {result.StatusCode}"), result);
                else
                    this.RequestSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                this.RequestError?.Invoke(e, null);
            }
        }

        /// <summary>
        ///     The structure of the data that is received for SSE events.
        /// </summary>
        public sealed class UpdatePayload
        {
            public string? ContentID { get; set; }
            public bool LoggedIn { get; set; }
            public uint HomeworldID { get; set; }
            public uint TerritoryID { get; set; }
        }

        /// <summary>
        ///     The structure of the data that is received from the API metadata endpoint.
        /// </summary>
        public sealed class MetadataPayload
        {
            public int connectedClients { get; set; } = 0;
            public string? donationPageUrl { get; set; } = null;
            public string? statusPageUrl { get; set; } = null;
            public string? newApiUrl = null;
        }

    }
}