namespace GoodFriend.Types;

using System;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Reflection;
using System.Timers;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.Utils;

/// <summary> The structure of the data that is received from the API for client events. </summary>
sealed public class UpdatePayload
{
    public string? ContentID { get; set; }
    public bool LoggedIn { get; set; }
}

/// <summary> An API client to built to communicate with the GoodFriend API. </summary>
public class APIClient : IDisposable
{
    ////////////////////////////
    /// Events and Delegates ///
    ////////////////////////////

    /// <summary> The Delegate that is used for DataReceieved. </summary>
    public delegate void DataRecievedDelegate(UpdatePayload data);

    /// <summary> Fired when data is recieved from the API. </summary>
    public event DataRecievedDelegate? DataRecieved;

    /// <summary> The Delegate that is used for Connection Estanlished events. </summary>
    public delegate void ConnectionEstablishedDelegate();

    /// <summary> Fired when a successful connection is established to the API. </summary>
    public event ConnectionEstablishedDelegate? ConnectionEstablished;

    /// <summary> // The Delegate that is used for Connection closed events. </summary> 
    public delegate void ConnectionClosedDelegate();

    /// <summary> Fired when the connection is closed. </summary>
    public event ConnectionClosedDelegate? ConnectionClosed;

    /// <summary> The Delegate that is used for ConnectionError </summary>
    public delegate void ConnectionErrorDelegate(Exception error);

    /// <summary> Fired when an error occurs with the connection. </summary>
    public event ConnectionErrorDelegate? ConnectionError;

    /// <summary> Handles a successful connection to the API. </summary>
    private void OnConnectionEstablished()
    {
        PluginLog.Log($"APIClient: Connection Established");

        // Set the state to connected and stop any reconnect timers.
        this.IsConnected = true;
        this._reconnectTimer.Stop();
    }

    /// <summary> Handles a connection closure (non-error). </summary>
    private void OnConnectionClosed()
    {
        PluginLog.Log($"APIClient: Connection to the API closed");

        // Set the state to disconnected & stop any reconnect timers.
        this.IsConnected = false;
        this._httpClient.CancelPendingRequests();
        this._reconnectTimer.Stop();
    }

    /// <summary> Handles a connection closer (on-error). </summary>
    private void OnConnectionError(Exception error)
    {
        PluginLog.Error($"APIClient: Connection error: {error.Message}");

        // Start attempting to reconnect to the API.
        this.IsConnected = false;
        this._httpClient.CancelPendingRequests();
        this._reconnectTimer.Start();
    }


    ////////////////////////////
    /// API Connection Config///
    ////////////////////////////

    /// <summary> The base of the API URL. </summary>
    private readonly Uri _apiUrl;

    /// <summary> The version of the API to use. </summary>
    private const string _apiVersion = "v1/";

    /// <summary> The place to send login data to the API. </summary>
    private const string _loginEndpoint = "login/";

    /// <summary> The place to send logout data to the API. </summary>
    private const string _logoutEndpoint = "logout/";

    /// <summary> The place to send get user events data from the API. </summary>
    private const string _userEventEndpoint = "events/users/";


    ////////////////////////////
    ///  HTTP Client Config  ///
    ////////////////////////////

    /// <summary> The HTTP client used to connect to the API. </summary>
    private HttpClient _httpClient = new HttpClient();

    /// <summary> Configures the HTTP client. </summary>
    private void ConfigureHttpClient()
    {
        this._httpClient.BaseAddress = new Uri(this._apiUrl + _apiVersion);
        this._httpClient.DefaultRequestHeaders.Add("User-Agent", $"Dalamud.{Common.RemoveWhitespace(PStrings.pluginName)}/{Assembly.GetExecutingAssembly().GetName().Version}");
        this._httpClient.Timeout = TimeSpan.FromSeconds(10);
        PluginLog.Debug($"APIClient: HTTPClient configured - BaseAddr: {this._httpClient.BaseAddress} - Agent: {this._httpClient.DefaultRequestHeaders.UserAgent}");
    }


    ////////////////////////////
    /// API Connection State ///
    ////////////////////////////

    /// <summary> The current state of the API connection. </summary>
    public bool IsConnected { get; private set; } = false;

    /// <summary> The timer for handling reconnection attempts. </summary>
    private Timer _reconnectTimer = new Timer(60000);

    /// <summary> The event handler for handling reconnection attempts. </summary>
    private void OnTryReconnect(object sender, ElapsedEventArgs e) => this.OpenStream();


    ////////////////////////////
    /// Construct n' Dispose ///
    ////////////////////////////

    /// <summary> Instantiates a new APIClient </summary>
    public APIClient(Uri url)
    {
        this._apiUrl = url;

        this.ConnectionEstablished += OnConnectionEstablished;
        this.ConnectionClosed += OnConnectionClosed;
        this.ConnectionError += OnConnectionError;
        this._reconnectTimer.Elapsed += OnTryReconnect;
        this.ConfigureHttpClient();
    }

    /// <summary> Disposes of the APIClient and its resources. </summary>
    public void Dispose()
    {
        // If we're still connected, disconecct.
        if (this.IsConnected) this.CloseStream();

        // Unregister any event handlers.
        this.ConnectionEstablished -= OnConnectionEstablished;
        this.ConnectionClosed -= OnConnectionClosed;
        this.ConnectionError -= OnConnectionError;
        this._reconnectTimer.Elapsed -= OnTryReconnect;

        // Dispose of all other resources.
        this._reconnectTimer.Dispose();
        this._httpClient.Dispose();

        PluginLog.Debug("APIClient: Successfully disposed.");
    }


    ////////////////////////////
    ///  API Data Receiving  ///
    ////////////////////////////

    /// <summary> Opens the connection stream to the API, throws error if already connected. </summary>
    public void OpenStream()
    {
        if (IsConnected) throw new InvalidOperationException("An active connection has already been established.");
        BeginStreamConnection();
    }

    /// <summary> Closes the connection stream to the API, throws error if not connected. </summary>
    public void CloseStream()
    {
        if (!IsConnected) throw new InvalidOperationException("There is no active connection to disconnect from.");
        ConnectionClosed?.Invoke();
    }

    /// <summary> Starts listening for data from the API </summary>
    private async void BeginStreamConnection()
    {
        try
        {
            // Try establishing a connection to the API.
            PluginLog.Log($"APIClient: Connecting to {this._httpClient.BaseAddress}{_userEventEndpoint}");

            using var stream = await this._httpClient.GetStreamAsync(_userEventEndpoint);
            using var reader = new StreamReader(stream);

            // Connection established! Start listening for data.
            ConnectionEstablished?.Invoke();

            // Wait for data to be recieved.
            while (!reader.EndOfStream && IsConnected)
            {
                // New message recieved! Parse it.
                var message = reader.ReadLine();
                if (message == null || message == " ") continue;

                // Remove any SSE (Server-Sent Events) filler characters.
                message = message.Replace("data: ", "").Trim();

                // Deserialize the object and fire the DataRecieved event if its valid.
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdatePayload>(message);
                if (data != null && data.ContentID != null) DataRecieved?.Invoke(data);
            }

            // We've reached the end of the stream, close up the connection.
            stream.Dispose();
        }

        catch (Exception e)
        {
            if (IsConnected) CloseStream();
            ConnectionError?.Invoke(e);
        }
    }


    ////////////////////////////
    ///   API Data Sending   ///
    ////////////////////////////

    /// <summary> Hashes and URL encodes the given data. </summary>
    private string HashAndEncode(string data) => HttpUtility.UrlEncode(Hashing.HashSHA512(data));

    /// <summary> Send a login event to the configured API/Login endpoint. </summary>
    public void SendLogin(ulong contentID)
    {
        this._httpClient.PostAsync($"{_loginEndpoint}{this.HashAndEncode(contentID.ToString())}", new StringContent(string.Empty)).ContinueWith(task =>
        {
            if (task.IsFaulted)
                PluginLog.Error($"APIClient: Failed to send login for player to {this._httpClient.BaseAddress}{_loginEndpoint}: {task.Exception?.Message}");

            else if (!task.Result.IsSuccessStatusCode)
                PluginLog.Error($"APIClient: Failed to send login for player to {this._httpClient.BaseAddress}{_loginEndpoint}: {task.Result.ReasonPhrase}");

            else if (task.IsCompleted)
                PluginLog.Log($"APIClient: Sent login for player to {this._httpClient.BaseAddress}{_loginEndpoint}");
        });
    }

    /// <summary> Send a logout event to the API/Logout endpoint. </summary>
    public void SendLogout(ulong contentID)
    {
        this._httpClient.PostAsync($"{_logoutEndpoint}{this.HashAndEncode(contentID.ToString())}", new StringContent(string.Empty)).ContinueWith(task =>
        {
            if (task.IsFaulted)
                PluginLog.Error($"APIClient: Failed to send logout for player to {this._httpClient.BaseAddress}{_loginEndpoint}: {task.Exception?.Message}");

            else if (!task.Result.IsSuccessStatusCode)
                PluginLog.Error($"APIClient: Failed to send logout for player to {this._httpClient.BaseAddress}{_logoutEndpoint}: {task.Result.ReasonPhrase}");

            else if (task.IsCompleted)
                PluginLog.Log($"APIClient: Sent logout for player to {this._httpClient}{_logoutEndpoint}");
        });
    }
}