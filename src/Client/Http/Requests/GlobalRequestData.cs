namespace GoodFriend.Client.Http.Requests
{
    public static class RequestConstants
    {
        public const string AuthenticationTokenHeader = "X-Auth-Token";
        public const string ContentIdHashHeader = "X-Content-Id-Hash";
        public const uint ContentIdHashMinLength = 64;
        public const uint ContentIdSaltMinLength = 32;
    }
}
