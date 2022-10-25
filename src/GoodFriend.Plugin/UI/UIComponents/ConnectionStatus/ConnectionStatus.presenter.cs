
namespace GoodFriend.UI.Components
{
    using System.Numerics;
    using GoodFriend.Base;
    using GoodFriend.Enums;
    using GoodFriend.Types;

    /// <summary>
    ///     The connection status modal for the API
    /// </summary>
    public sealed class ConnectionStatusPresenter
    {
        public ConnectionStatus APIStatus => PluginService.APIClientManager.GetConnectionStatus();

        public string GetConnectionStatusText()
        {
            return this.APIStatus switch
            {
                ConnectionStatus.Connected => State.Connected,
                ConnectionStatus.Connecting => State.Connecting,
                ConnectionStatus.Ratelimited => State.Ratelimited,
                ConnectionStatus.Disconnected => State.Disconnected,
                ConnectionStatus.Error => State.ConnectionError,
                _ => State.Unknown,
            };
        }

        public Vector4 GetConnectionStatusColour()
        {
            return this.APIStatus switch
            {
                ConnectionStatus.Connected => Components.Colours.APIConnected,
                ConnectionStatus.Connecting => Components.Colours.APIConnecting,
                ConnectionStatus.Ratelimited => Components.Colours.APIRatelimited,
                ConnectionStatus.Disconnected => Components.Colours.APIDisconnected,
                ConnectionStatus.Error => Components.Colours.APIError,
                _ => Components.Colours.Error,
            };
        }

        public string GetConnectionStatusDescription()
        {
            return this.APIStatus switch
            {
                ConnectionStatus.Connected => State.ConnectedDescription(this.metadata?.connectedClients ?? 0),
                ConnectionStatus.Ratelimited => State.RatelimitedDescription,
                ConnectionStatus.Connecting => State.ConnectingDescription,
                ConnectionStatus.Disconnected => State.DisconnectedDescription,
                ConnectionStatus.Error => State.ConnectionErrorDescription,
                _ => State.UnknownDescription,
            };
        }

        public APIClient.MetadataPayload? metadata => PluginService.APIClientManager.metadataCache;
    }
}
