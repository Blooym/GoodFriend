namespace GoodFriend.Client.Requests
{
    /// <summary>
    ///     Represents data that is used across multiple requests.
    /// </summary>
    public static class GlobalRequestData
    {
        public const uint ContentIdHashMinLength = 64;
        public const uint ContentIdSaltMinLength = 32;
    }
}