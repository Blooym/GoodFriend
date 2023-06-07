namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents data that is used across multiple requests.
    /// </summary>
    internal static class GlobalRequestData
    {
        internal const uint ContentIdHashMinLength = 64;
        internal const uint ContentIdSaltMinLength = 32;
    }
}
