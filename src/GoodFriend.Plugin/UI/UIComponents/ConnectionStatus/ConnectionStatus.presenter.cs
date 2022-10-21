
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
                ConnectionStatus.Connected => TStrings.StateConnected,
                ConnectionStatus.Connecting => TStrings.StateConnecting,
                ConnectionStatus.Ratelimited => TStrings.StateRatelimited,
                ConnectionStatus.Disconnected => TStrings.StateDisconnected,
                ConnectionStatus.Error => TStrings.StateConnectionError,
                _ => TStrings.StateUnknown
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
                ConnectionStatus.Connected => TStrings.StateConnectedDescription(PluginService.APIClientManager.GetMetadata()?.connectedClients ?? 0),
                ConnectionStatus.Ratelimited => TStrings.StateRatelimitedDescription,
                ConnectionStatus.Connecting => TStrings.StateConnectingDescription,
                ConnectionStatus.Disconnected => TStrings.StateDisconnectedDescription,
                ConnectionStatus.Error => TStrings.StateConnectionErrorDescription,
                _ => TStrings.StateUnknownDescription,
            };
        }

        public APIClient.MetadataPayload? GetMetadata => PluginService.APIClientManager.GetMetadata();
    }
}
