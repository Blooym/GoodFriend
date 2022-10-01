namespace GoodFriend.Types;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Reflection;
using System.Timers;
using Dalamud.Logging;
using Newtonsoft.Json;
using GoodFriend.Base;
using GoodFriend.Utils;


/// <summary>
///     An API client to built to communicate with the GoodFriend API.
/// </summary>
public class APIClient : IDisposable
{
    ////////////////////////////
    /// Events and Delegates ///
    ////////////////////////////

    /// <summary>
    ///     The Delegate that is used for DataReceieved.
    /// </summary>
    public delegate void DataRecievedDelegate(UpdatePayload data);

    /// <summary>
    ///     Fired when data is recieved from the API.
    /// </summary>
    public event DataRecievedDelegate? DataRecieved;

    /// <summary>
    ///     The Delegate that is used for Connection Estanlished events.
    /// </summary>
    public delegate void ConnectionEstablishedDelegate();

    /// <summary>
    ///     Fired when a successful connection is established to the API.
    /// </summary>
    public event ConnectionEstablishedDelegate? ConnectionEstablished;

    /// <summary>
    //      The Delegate that is used for Connection closed events.
    /// </summary> 
    public delegate void ConnectionClosedDelegate();

    /// <summary>
    ///     Fired when the connection is closed.
    /// </summary>
    public event ConnectionClosedDelegate? ConnectionClosed;

    /// <summary> 
    ///     The Delegate that is used for ConnectionError. 
    /// </summary>
    public delegate void ConnectionErrorDelegate(Exception error);

    /// <summary>
    ///     Fired when an error occurs with the connection.
    /// </summary>
    public event ConnectionErrorDelegate? ConnectionError;

    /// <summary>
    ///     Handles a successful connection to the API.
    /// </summary>
    private void OnConnectionEstablished()
    {
        PluginLog.Log($"APIClient(OnConnectionEstablished): Connection Established");

        // Set the state to connected and stop any reconnect timers.
        this.IsConnected = true;
        this._reconnectTimer.Stop();
    }

    /// <summary>
    ///     Handles a connection closure (non-error).
    /// </summary>
    private void OnConnectionClosed()
    {
        PluginLog.Log($"APIClient(OnConnectionClosed): Connection to the API closed");

        // Set the state to disconnected & stop any reconnect timers.
        this.IsConnected = false;
        this._httpClient.CancelPendingRequests();
        this._reconnectTimer.Stop();
    }

    /// <summary>
    ///     Handles a connection closer (on-error).
    /// </summary>
    private void OnConnectionError(Exception error)
    {
        PluginLog.Error($"APIClient(OnConnectionError): Connection error: {error.Message}");

        // Start attempting to reconnect to the API.
        this.IsConnected = false;
        this._httpClient.CancelPendingRequests();
        this._reconnectTimer.Start();
    }


    ////////////////////////////
    /// API Connection Config///
    ////////////////////////////

    /// <summary> 
    ///     The base of the API URL.
    /// </summary>
    private readonly Configuration _configuration;

    /// <summary>
    ///     The version of the API to use.
    /// </summary>
    private const string _apiVersion = "v3/";

    /// <summary>
    ///     The place to send login data to the API.
    /// </summary>
    private const string _loginEndpoint = "login";

    /// <summary>
    ///     The place to send logout data to the API.
    /// </summary>
    private const string _logoutEndpoint = "logout";

    /// <summary>
    ///     The place to send get user events data from the API.
    /// </summary>
    private const string _userEventEndpoint = "events/users/";

    /// <summary>
    ///     The place to get the amount of clients connected to the API.
    /// </summary>
    private const string _clientCountEndpoint = "clients/";


    ////////////////////////////
    ///  HTTP Client Config  ///
    ////////////////////////////

    /// <summary>
    ///     The HTTP client used to connect to the API.
    /// </summary>
    private HttpClient _httpClient = new HttpClient();

    /// <summary>
    ///     Configures the HTTP client.
    /// </summary>
    private void ConfigureHttpClient()
    {
        this._httpClient.DefaultRequestHeaders.Add("User-Agent", $"Dalamud.{Common.RemoveWhitespace(PStrings.pluginName)}/{Assembly.GetExecutingAssembly().GetName().Version}");
        this._httpClient.DefaultRequestHeaders.Add("session-identifier", Guid.NewGuid().ToString());
        if (this._configuration.APIBearerToken != string.Empty) this._httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this._configuration.APIBearerToken}");
        this._httpClient.Timeout = TimeSpan.FromSeconds(10);
        this._httpClient.BaseAddress = new Uri(this._configuration.APIUrl + _apiVersion);
        PluginLog.Debug($"APIClient(ConfigureHttpClient): HTTPClient configured - BaseAddr: {this._httpClient.BaseAddress} - Headers: {this._httpClient.DefaultRequestHeaders.ToString()}");
    }


    ////////////////////////////
    /// API Connection State ///
    ////////////////////////////

    /// <summary>
    ///    The current state of the API connection.
    /// </summary>
    public bool IsConnected { get; private set; } = false;

    /// <summary>
    ///     The timer for handling reconnection attempts.
    /// </summary>
    private Timer _reconnectTimer = new Timer(60000);

    /// <summary>
    ///     The event handler for handling reconnection attempts.
    /// </summary>
    private void OnTryReconnect(object? sender, ElapsedEventArgs e) => this.OpenStream();

    /// <summary>
    ///     The amount of clients connected to the API.
    /// </summary>
    public int ConnectedClients { get; private set; } = 0;

    /// <summary>
    ///     Force fetch the amount of clients connected to the API and return the value.
    /// </summary>
    public int GetClientCount() { this.ConnectedClients = this.GetConnectedClients(); return this.ConnectedClients; }


    ////////////////////////////
    /// Construct n' Dispose ///
    ////////////////////////////


    /// <summary>
    ///     Instantiates a new APIClient
    /// </summary>
    public APIClient(Configuration config)
    {
        this._configuration = config;

        this.ConnectionEstablished += OnConnectionEstablished;
        this.ConnectionClosed += OnConnectionClosed;
        this.ConnectionError += OnConnectionError;
        this._reconnectTimer.Elapsed += OnTryReconnect;
        this.ConfigureHttpClient();
    }

    /// <summary>
    ///     Disposes of the APIClient and its resources.
    /// </summary>
    public void Dispose()
    {
        // If we're still connected, disconecct.
        if (this.IsConnected) this.CloseStream();

        // Unregister any event handlers.
        this.ConnectionEstablished -= OnConnectionEstablished;
        this.ConnectionClosed -= OnConnectionClosed;
        this.ConnectionError -= OnConnectionError;
        this._reconnectTimer.Elapsed -= OnTryReconnect;

        // Cancel pending requests.
        this._httpClient.CancelPendingRequests();

        // Dispose of all other resources.
        this._reconnectTimer.Dispose();
        this._httpClient.Dispose();


        PluginLog.Debug("APIClient(Dispose): Successfully disposed.");
    }


    ////////////////////////////
    ///   Utility Methods    ///
    ////////////////////////////

    /// <summary>
    ///     Adds a warning to the log when the API reports itself as deprecated.
    /// </summary>
    private void WarnOnDeprecated(HttpResponseMessage response)
    {
        if (response.Headers.Contains("Deprecated") && response.Headers.GetValues("Deprecated").First() == "true")
            PluginLog.Warning("APIClient(WarnOnDeprecated): API reports that this version of the API is deprecated and will be removed in the future, consider updating.");
    }


    ////////////////////////////
    ///  API Data Receiving  ///
    ////////////////////////////

    /// <summary> 
    ///     Opens the connection stream to the API, throws error if already connected.
    /// </summary>
    public void OpenStream()
    {
        if (IsConnected) throw new InvalidOperationException("APIClient(OpenStream): An active connection has already been established.");
        this.BeginStreamConnection();
    }

    /// <summary>
    //      Closes the connection stream to the API, throws error if not connected.
    /// </summary>
    public void CloseStream()
    {
        if (!IsConnected) throw new InvalidOperationException("APIClient(CloseStream): There is no active connection to disconnect from.");
        this.ConnectionClosed?.Invoke();
    }

    /// <summary>
    ///     Starts listening for data from the API.
    /// </summary>
    private async void BeginStreamConnection()
    {
        try
        {
            // Try establishing a connection to the API.
            PluginLog.Log($"APIClient(BeginStreamConnection): Connecting to {this._httpClient.BaseAddress}{_userEventEndpoint}");

            using var stream = await this._httpClient.GetStreamAsync(_userEventEndpoint);
            using var reader = new StreamReader(stream);

            // Connection established! Start listening for data.
            this.ConnectionEstablished?.Invoke();

            // Wait for data to be recieved.
            while (!reader.EndOfStream && IsConnected)
            {
                // Read the message, if its null or just a colon (:) then skip it.
                // Colon indicates a heartbeat message.
                var message = reader.ReadLine();
                if (message == null || message.Trim() == ":") continue;

                // Remove any SSE (Server-Sent Events) filler characters.
                message = message.Replace("data: ", "").Trim();

                // Deserialize the object and fire the DataRecieved event if its valid.
                var data = JsonConvert.DeserializeObject<UpdatePayload>(message);
                if (data != null && data.ContentID != null) DataRecieved?.Invoke(data);
            }

            stream.Close();
            stream.Dispose();

            if (reader.EndOfStream && IsConnected)
            {
                PluginLog.Log("APIClient(BeginStreamConnection): Connection closed by server.");
                this.CloseStream();
            }
        }

        catch (Exception e)
        {
            if (IsConnected) CloseStream();
            this.ConnectionError?.Invoke(e);
        }
    }

    /// <summary>
    ///     Fetches the amount of clients connected to the API.
    /// </summary>
    private int GetConnectedClients()
    {
        int connected = 0;

        var request = new HttpRequestMessage
        (
            HttpMethod.Get,
            $"{_clientCountEndpoint}"
        );

        var task = this._httpClient.SendAsync(request).ContinueWith(async (task) =>
        {
            if (task.IsFaulted)
            {
                PluginLog.Error($"APIClient(GetConnectedClients): Failed to get connected clients from {request.RequestUri}: {task.Exception?.Message}");
                connected = this.ConnectedClients;
            }

            else if (!task.Result.IsSuccessStatusCode)
            {
                PluginLog.Warning($"APIClient(GetConnectedClients): Failed to get connected clients from {request.RequestUri}: {task.Result.ReasonPhrase} ({task.Result.Content.ReadAsStringAsync().Result})");
                connected = this.ConnectedClients;
            }

            else if (task.IsCompleted)
            {
                var response = await task.Result.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<ClientAmountPayload>(response);
                if (json == null) connected = this.ConnectedClients;
                else connected = json.clients;
            }
        });

        task.Wait();

        if (task.IsCompletedSuccessfully) { PluginLog.Debug($"APIClient: {request.RequestUri} reports {connected} clients connected."); return connected; }
        else { return this.ConnectedClients; }
    }


    ////////////////////////////
    ///   API Data Sending   ///
    ////////////////////////////

    /// <summary>
    ///     Send a login event to the configured API/Login endpoint.
    /// </summary>
    public void SendLogin(ulong contentID, uint homeworldID, uint territoryID)
    {
        var request = new HttpRequestMessage
        (
            HttpMethod.Put,
            $"{_loginEndpoint}?contentID={HttpUtility.UrlEncode(Hashing.HashSHA512(contentID.ToString()))}&homeworldID={homeworldID}&territoryID={territoryID}"
        );

        this._httpClient.SendAsync(request).ContinueWith(task =>
        {
            if (task.IsFaulted)
                PluginLog.Error($"APIClient(SendLogin): Failed to send status update to{request.RequestUri}: {task.Exception?.Message}");

            else if (!task.Result.IsSuccessStatusCode)
                PluginLog.Warning($"APIClient(SendLogin): Failed to send status update to {request.RequestUri}: {task.Result.ReasonPhrase} ({task.Result.Content.ReadAsStringAsync().Result})");

            else if (task.IsCompleted)
                PluginLog.Log($"APIClient(SendLogin): Sent status update to {request.RequestUri}");

            WarnOnDeprecated(task.Result);
        });
    }


    /// <summary>
    ///     Send a logout event to the configured API/logout endpoint.
    /// </summary>
    public void SendLogout(ulong contentID, uint homeworldID, uint territoryID)
    {
        var request = new HttpRequestMessage
        (
            HttpMethod.Put,
            $"{_logoutEndpoint}?contentID={HttpUtility.UrlEncode(Hashing.HashSHA512(contentID.ToString()))}&homeworldID={homeworldID}&territoryID={territoryID}"
        );

        this._httpClient.SendAsync(request).ContinueWith(task =>
        {
            if (task.IsFaulted)
                PluginLog.Error($"APIClient(SendLogout): Failed to send status update to{request.RequestUri}: {task.Exception?.Message}");

            else if (!task.Result.IsSuccessStatusCode)
                PluginLog.Warning($"APIClient(SendLogout): Failed to send status update to {request.RequestUri}: {task.Result.ReasonPhrase} ({task.Result.Content.ReadAsStringAsync().Result})");

            else if (task.IsCompleted)
                PluginLog.Log($"APIClient(SendLogout): Sent status update to {request.RequestUri}");

            WarnOnDeprecated(task.Result);
        });
    }
}

/// <summary>
///     The structure of the data that is received from the API for client events.
/// </summary>
sealed public class UpdatePayload
{
    public string? ContentID { get; set; }
    public bool LoggedIn { get; set; }
    public uint HomeworldID { get; set; }
    public uint TerritoryID { get; set; }
}

/// <summary>
///     The structure of the data that is received from the API for client counts.
/// </summary>
sealed public class ClientAmountPayload
{
    public int clients { get; set; }
}
