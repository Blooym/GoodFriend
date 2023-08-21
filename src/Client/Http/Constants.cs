using System.Net.Http;
using System.Reflection;

namespace GoodFriend.Client.Http
{
    /// <summary>
    ///     Utility class for <see cref="HttpClient" />-related constants.
    /// </summary>
    public static class HttpConstants
    {
        /// <summary>
        ///     The recommended user agent to use for requests.
        /// </summary>
        public static string UserAgent { get; } = $"GoodFriend.Client/{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
