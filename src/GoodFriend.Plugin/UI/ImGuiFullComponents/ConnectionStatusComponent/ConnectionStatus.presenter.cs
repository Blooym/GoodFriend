using System.Numerics;
using GoodFriend.Base;
using GoodFriend.Enums;
using GoodFriend.Localization;
using GoodFriend.Types;
using GoodFriend.UI.ImGuiComponents;

namespace GoodFriend.UI.ImGuiFullComponents.ConnectionStatusComponent
{
    /// <summary>
    ///     The connection status modal for the API
    /// </summary>
    public sealed class ConnectionStatusPresenter
    {
        public static ConnectionStatus APIStatus => PluginService.APIClientManager.GetConnectionStatus();

        public static string GetConnectionStatusText()
        {
            return APIStatus switch
            {
                ConnectionStatus.Connected => State.Connected,
                ConnectionStatus.Connecting => State.Connecting,
                ConnectionStatus.Ratelimited => State.Ratelimited,
                ConnectionStatus.Disconnected => State.Disconnected,
                ConnectionStatus.Error => State.ConnectionError,
                _ => State.Unknown,
            };
        }

        public static Vector4 GetConnectionStatusColour()
        {
            return APIStatus switch
            {
                ConnectionStatus.Connected => Colours.APIConnected,
                ConnectionStatus.Connecting => Colours.APIConnecting,
                ConnectionStatus.Ratelimited => Colours.APIRatelimited,
                ConnectionStatus.Disconnected => Colours.APIDisconnected,
                ConnectionStatus.Error => Colours.APIError,
                _ => Colours.Error,
            };
        }

        public static string GetConnectionStatusDescription()
        {
            return APIStatus switch
            {
                ConnectionStatus.Connected => State.ConnectedDescription(Metadata?.ConnectedClients ?? 0),
                ConnectionStatus.Ratelimited => State.RatelimitedDescription,
                ConnectionStatus.Connecting => State.ConnectingDescription,
                ConnectionStatus.Disconnected => State.DisconnectedDescription,
                ConnectionStatus.Error => State.ConnectionErrorDescription,
                _ => State.UnknownDescription,
            };
        }

        public static APIClient.MetadataPayload? Metadata => PluginService.APIClientManager.MetadataCache;
    }
}
