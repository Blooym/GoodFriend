namespace GoodFriend.Managers;

using System;
using System.IO;
using System.Net.Http;
using System.Web;
using Dalamud.Logging;
using GoodFriend.Base;
using GoodFriend.Utils;

/// <summary>
///    The structure of the data that is received from the API for client events.
/// </summary>
sealed public class Update
{
    public string? ContentID { get; set; }
    public bool? LoggedIn { get; set; }
}

/// <summary>
///    Manages the client connection and data handling to/from the API.
/// </summary>
sealed public class APIClientManager : IDisposable
{
    /// <summary> The Delegate that is used for DataReceieved. </summary>
    public delegate void DataRecievedDelegate(Update data);

    /// <summary> Fired when data is recieved from the API. </summary>
    public event DataRecievedDelegate? DataRecieved;

    /// <summary> The Delegate that is used for Connection Estanlished events. </summary>
    public delegate void ConnectionEstablishedDelegate();

    /// <summary> Fired when data is first recieved from the API after a connection.</summary>
    public event ConnectionEstablishedDelegate? ConnectionEstablished;

    /// <summary> // The Delegate that is used for Connection closed events. </summary> 
    public delegate void ConnectionClosedDelegate();

    /// <summary> Fired when the connection is terminated.</summary>
    public event ConnectionClosedDelegate? ConnectionClosed;

    /// <summary> The Delegate that is used for when an error occurs whilst connecting </summary>
    public delegate void ConnectionErrorDelegate(Exception error);

    /// <summary> Fired when an error occurs whilst connecting. </summary>
    public event ConnectionErrorDelegate? ConnectionError;

    /// <summary> The API URLs to manage events with. </summary>
    private static readonly Uri _apiUrl = PluginService.Configuration.APIUrl;
    private static readonly string _apiVersion = "v1";
    private static readonly string _loginUrl = $"{_apiUrl}{_apiVersion}/login";
    private static readonly string _logoutUrl = $"{_apiUrl}{_apiVersion}/logout";
    private static readonly string _userEventsUrl = $"{_apiUrl}{_apiVersion}/events/users";


    /// <summary> The status of the connection. </summary>
    public bool IsConnected { get; private set; } = false;


    /// <summary> Sets the connection to true when established. </summary>
    private void OnConnectionEstablished() => this.IsConnected = true;


    /// <summary> Sets the connection to false when closed, regardless of reason. </summary>
    private void OnConnectionClosed() => this.IsConnected = false;


    /// <summary> Fired when an error occurs whilst connecting. </summary>
    private void OnConnectionError(Exception error) => PluginLog.Error($"APIClientManager: Connection error: {error.ToString()}");


    /// <summary>
    ///     Instantiates the APIClientManager.
    /// </summary>
    public APIClientManager()
    {
        PluginLog.Debug("APIClientManager: Instantiating...");

        this.ConnectionEstablished += OnConnectionEstablished;
        this.ConnectionClosed += OnConnectionClosed;
        this.ConnectionError += OnConnectionError;

        PluginLog.Debug("APIClientManager: Instantiation complete.");
    }

    /// <summary>
    ///     Opens the connection to the API.
    ///     It should be checked to see if there is not an active connection before calling this.
    /// </summary>
    public void Connect()
    {
        if (IsConnected) throw new InvalidOperationException("An active connection has already been established.");
        BeginConnection();
    }


    /// <summary>
    ///     Kills the connection to the API
    ///     It should be checked to see if there is an active connection before calling this.
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected) throw new InvalidOperationException("There is no active connection to disconnect from.");
        ConnectionClosed?.Invoke();
    }


    /// <summary>
    ///     Disposes of the ClientManager.
    /// </summary>
    public void Dispose()
    {
        PluginLog.Debug("APIClientManager: Disposing...");

        if (this.IsConnected) this.Disconnect();

        this.ConnectionEstablished -= OnConnectionEstablished;
        this.ConnectionClosed -= OnConnectionClosed;
        this.ConnectionError -= OnConnectionError;

        PluginLog.Debug("APIClientManager: Successfully disposed.");
    }


    /// <summary>
    ///     Send a login event to the API.
    /// </summary>
    public void SendLogin(ulong contentID)
    {
        string ID = HttpUtility.UrlEncode(Hashing.HashSHA512(contentID.ToString()));
        new HttpClient().PostAsync($"{_loginUrl}/{ID}", new StringContent(string.Empty)).ContinueWith(task =>
        {
            if (task.IsFaulted)
                PluginLog.Error($"APIClientManager: Failed to send login for player to {_loginUrl}: {task.Exception?.Message}");

            else if (!task.Result.IsSuccessStatusCode)
                PluginLog.Error($"APIClientManager: Failed to send login for player to {_loginUrl}: {task.Result.ReasonPhrase}");

            else if (task.IsCompleted)
                PluginLog.Log($"APIClientManager: Sent login for player to {_loginUrl}");
        });
    }


    /// <summary>
    ///     Send a logout event to the API.
    /// </summary>
    public void SendLogout(ulong contentID)
    {
        string ID = HttpUtility.UrlEncode(Hashing.HashSHA512(contentID.ToString()));
        new HttpClient().PostAsync($"{_logoutUrl}/{ID}", new StringContent(string.Empty)).ContinueWith(task =>
        {
            if (task.IsFaulted)
                PluginLog.Error($"APIClientManager: Failed to send logout for player to {_logoutUrl}: {task.Exception?.Message}");

            else if (!task.Result.IsSuccessStatusCode)
                PluginLog.Error($"APIClientManager: Failed to send logout for player to {_logoutUrl} {task.Result.ReasonPhrase}");

            else if (task.IsCompleted)
                PluginLog.Log($"APIClientManager: Sent logout for playerto {_logoutUrl}");
        });
    }


    /// <summary>
    ///     Starts listening for data from the API.
    /// </summary>
    private async void BeginConnection()
    {
        try
        {
            // Try establishing a connection to the API.
            PluginLog.Log($"APIClientManager: Connecting to {_userEventsUrl}");
            var client = new HttpClient();
            client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;

            var stream = await client.GetStreamAsync($"{_userEventsUrl}");
            var reader = new StreamReader(stream);

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
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Update>(message);
                if (data != null && data.LoggedIn != null && data.ContentID != null) DataRecieved?.Invoke(data);
            }

            // We've reached the end of the stream, close up the connection.
            stream.Dispose();
        }

        catch (Exception e)
        {
            if (IsConnected) Disconnect();
            ConnectionError?.Invoke(e);
        }
    }
}