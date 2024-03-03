using System.Net.Http;
using System.Threading.Tasks;
using GoodFriend.Client.Http.Interfaces;

namespace GoodFriend.Client.Http.Requests
{
    /// <summary>
    ///     Request to validate an authentication token.
    /// </summary>
    public class PostValidateAuthRequest : IHttpRequestHandler<PostValidateAuthRequest.RequestData, (bool, HttpResponseMessage)>
    {
        private const string EndpointUrl = "api/auth/validate";

        public readonly record struct RequestData
        {
            public readonly string AuthenticationToken { get; init; }
        }

        /// <summary>
        ///     Builds the request message.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildMessage(RequestData requestData) => new(HttpMethod.Post, EndpointUrl)
        {
            Headers =
            {
                { GlobalRequestData.Headers.AuthenticationToken, requestData.AuthenticationToken },
            },
        };

        /// <inheritdoc />
        public (bool, HttpResponseMessage) Send(HttpClient httpClient, RequestData requestData)
        {
            var request = BuildMessage(requestData);
            var response = httpClient.Send(request);
            return (response.IsSuccessStatusCode, response);
        }

        /// <inheritdoc />
        public Task<(bool, HttpResponseMessage)> SendAsync(HttpClient httpClient, RequestData requestData)
        {
            var request = BuildMessage(requestData);
            var response = httpClient.SendAsync(request);
            return Task.FromResult((response.Result.IsSuccessStatusCode, response.Result));
        }
    }
}
