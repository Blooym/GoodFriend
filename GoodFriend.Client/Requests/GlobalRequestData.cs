namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents data that is used across multiple requests.
    /// </summary>
    internal static class GlobalRequestData
    {
        internal const uint CONTENT_ID_HASH_MIN_LENGTH = 64;
        internal const uint CONTENT_ID_SALT_MIN_LENGTH = 32;
    }
}