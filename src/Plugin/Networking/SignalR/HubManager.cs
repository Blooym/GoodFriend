using System;
using System.Threading;
using System.Threading.Tasks;
using GoodFriend.Plugin.Base;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace GoodFriend.Networking.SignalR;

internal sealed class HubManager : IDisposable
{
    public readonly HubConnection PlayerEventHub;

    public HubManager() => this.PlayerEventHub = new HubConnectionBuilder()
       .AddMessagePackProtocol(opt =>
           opt.SerializerOptions
           .WithCompression(MessagePackCompression.Lz4Block)
           .WithResolver(CompositeResolver.Create(
               StandardResolver.Instance,
               BuiltinResolver.Instance,
               AttributeFormatterResolver.Instance,
               DynamicEnumAsStringResolver.Instance,
               DynamicGenericResolver.Instance,
               DynamicUnionResolver.Instance,
               DynamicObjectResolver.Instance,
               PrimitiveObjectResolver.Instance,
               StandardResolver.Instance
           )))
       .WithAutomaticReconnect(new ForeverRetryPolicy())
       .WithUrl(new Uri(Services.PluginConfiguration.ApiConfig.BaseUrl, "/hubs/playerevents"))
       .Build();

    /// <inheritdoc />
    public void Dispose() => this.PlayerEventHub.DisposeAsync().AsTask().GetAwaiter().GetResult();

    /// <summary>
    ///     Start the connection for the provided hub and retry on initial failure.
    /// </summary>
    public static async void StartAsyncWithRetry(HubConnection hub, CancellationToken cancellationToken)
    {
        var connected = false;
        var policy = new ForeverRetryPolicy();
        var retryContext = new RetryContext();

        try
        {
            while (!connected && hub.State is HubConnectionState.Disconnected)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await hub.StartAsync(cancellationToken);
                    Logger.Information($"Connection to hub established - id: {hub.ConnectionId}");
                    connected = true;
                }
                catch (Exception ex)
                {
                    var reconnectTimer = policy.NextRetryDelay(retryContext);
                    retryContext.PreviousRetryCount++;
                    if (!reconnectTimer.HasValue)
                    {
                        break;
                    }
                    Logger.Error($"Connection attempt failed. Retrying in {reconnectTimer} - error: {ex.Message}");
                    await Task.Delay(reconnectTimer.Value, cancellationToken);
                    retryContext.ElapsedTime += reconnectTimer.Value;
                }
            }
        }
        catch (OperationCanceledException cancel)
        {
            Logger.Information($"StartAsyncWithRetry was cancelled via cancellation token - {cancel.Message}");
        }
    }
}
