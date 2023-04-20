using System.Numerics;
using GoodFriend.Client;
using GoodFriend.Client.Responses;
using GoodFriend.Plugin.Common;
using GoodFriend.Plugin.Localization;
using GoodFriend.Plugin.UserInterface.ImGuiBasicComponents;

namespace GoodFriend.Plugin.UserInterface.ImGuiFullComponents.ConnectionStatusComponent
{
    /// <summary>
    ///     The connection status modal for the API
    /// </summary>
    public sealed class ConnectionStatusPresenter
    {
        public static EventStreamConnectionState APIStatus => Services.APIClientManager.ConnectionStatus;

        public static string GetConnectionStatusText() => APIStatus switch
        {
            EventStreamConnectionState.Connected => State.Connected,
            EventStreamConnectionState.Connecting => State.Connecting,
            EventStreamConnectionState.Disconnected => State.Disconnected,
            EventStreamConnectionState.Exception => State.ConnectionError,
            EventStreamConnectionState.Disconnecting => throw new System.NotImplementedException(),
            _ => State.Unknown,
        };

        public static Vector4 GetConnectionStatusColour() => APIStatus switch
        {
            EventStreamConnectionState.Connected => Colours.APIConnected,
            EventStreamConnectionState.Connecting => Colours.APIConnecting,
            EventStreamConnectionState.Disconnected => Colours.APIDisconnected,
            EventStreamConnectionState.Exception => Colours.APIError,
            EventStreamConnectionState.Disconnecting => throw new System.NotImplementedException(),
            _ => Colours.Error,
        };

        public static string GetConnectionStatusDescription() => APIStatus switch
        {
            EventStreamConnectionState.Connected => State.ConnectedDescription(Metadata?.ConnectedClients ?? 0),
            EventStreamConnectionState.Connecting => State.ConnectingDescription,
            EventStreamConnectionState.Disconnected => State.DisconnectedDescription,
            EventStreamConnectionState.Exception => State.ConnectionErrorDescription,
            EventStreamConnectionState.Disconnecting => throw new System.NotImplementedException(),
            _ => State.UnknownDescription,
        };

        public static MetadataResponse? Metadata => Services.APIClientManager.MetadataCache;
    }
}
