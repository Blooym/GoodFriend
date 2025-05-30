using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GoodFriend.Server.Hubs;

public class PlayerEventHub(ILogger<PlayerEventHub> logger) : Hub
{
    private static readonly Action<ILogger, string, string, uint, ushort, bool, Exception?> LogLoginEvent = LoggerMessage.Define<string, string, uint, ushort, bool>(
        LogLevel.Information,
        new EventId(1, "SendLoginEvent"),
        "Received Login Event: ContentIdHash={ContentIdHash}, ContentIdSalt={ContentIdSalt}, WorldId={WorldId}, TerritoryId={TerritoryId}, LoggedIn={LoggedIn}"
    );

    private readonly ILogger<PlayerEventHub> logger = logger;
    private readonly LinkedList<string> seenContentIds = [];

    public async Task SendLoginEvent(string contentIdHash, string contentIdSalt, uint worldId, ushort territoryId, bool loggedIn)
    {
        if (string.IsNullOrWhiteSpace(contentIdHash))
        {
            throw new HubException("ContentIdHash is required");
        }
        if (string.IsNullOrWhiteSpace(contentIdSalt))
        {
            throw new HubException("ContentIdSalt is required");
        }
        if (this.seenContentIds.Contains(contentIdHash))
        {
            throw new HubException("Duplicate ContentId hash sent to server");
        }

        this.seenContentIds.AddFirst(contentIdHash);
        if (this.seenContentIds.Count > 500)
        {
            this.seenContentIds.RemoveLast();
        }
        LogLoginEvent(this.logger, contentIdHash, contentIdSalt, worldId, territoryId, loggedIn, null);
        await this.Clients.Others.SendAsync("ReceiveLoginEvent", contentIdHash, contentIdSalt, worldId, territoryId, loggedIn);
    }
}
