using System;
using System.Net.Http;
using System.Reflection;

namespace GoodFriend.Client.Http
{
    /// <summary>
    ///     Utility class for creating <see cref="HttpClient"/> instances.
    /// </summary>
    public static class HttpClientUtil
    {
        /// <summary>
        ///     Creates a new <see cref="HttpClient"/> instance.
        /// </summary>
        /// <param name="baseAddress">The base address of the client.</param>
        /// <returns></returns>
        public static HttpClient CreateHttpClient(Uri baseAddress)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = baseAddress,
                DefaultRequestHeaders = {
                    { "User-Agent", $"GoodFriend.Client/{Assembly.GetExecutingAssembly().GetName().Version}" },
                },
                Timeout = TimeSpan.FromSeconds(10),
            };
            return httpClient;
        }
    }
}
