namespace GoodFriend.Client.Http.Requests;

public static class GlobalRequestData
{
    public static class Headers
    {
        public const string AuthenticationToken = "X-Auth-Token";
        public const string ContentIdHash = "X-Content-Id-Hash";
        public const string ClientKey = "X-Client-Key";
        public const string ContentIdSalt = "X-Content-Id-Salt";
    }
    public static class Validation
    {
        public const uint ContentIdHashMinLength = 64;
        public const uint ContentIdSaltMinLength = 32;
    }
}
