namespace GoodFriend.Enums
{
    /// <summary>
    ///     All of the available connection statuses.
    /// </summary>
    public enum ConnectionStatus
    {
        Connected = 0,
        Connecting = 1,
        Disconnected = 2,
        Ratelimited = 3,
        Error = 4,
    }
}
