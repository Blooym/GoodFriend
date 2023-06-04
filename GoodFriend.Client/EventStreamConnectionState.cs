namespace GoodFriend.Client
{
    public enum EventStreamConnectionState
    {
        /// <summary>
        ///     The client is currently connected to the event stream.
        /// </summary>
        Connected,

        /// <summary>
        ///     The client is currently connecting to the event stream.
        /// </summary>
        Connecting,

        /// <summary>
        ///     The client is currently disconnecting from the event stream.
        /// </summary>
        Disconnecting,

        /// <summary>
        ///     The client is not currently connected to the event stream.
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Something went wrong with the event stream.
        /// </summary>
        Exception,
    }
}
