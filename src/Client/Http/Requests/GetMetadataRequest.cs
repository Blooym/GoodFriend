using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GoodFriend.Client.Http.Responses;

namespace GoodFriend.Client.Http.Requests
{
    public class GetMetadataRequest : IGoodFriendRequestHandler<GetMetadataRequest.RequestData, (MetadataResponse, HttpResponseMessage)>
    {
        private const string EndpointUrl = "api/metadata";

        /// <summary>
        ///     Represents the request data for the metadata request.
        /// </summary>
        public readonly record struct RequestData
        {
            public static RequestData None { get; }
        }

        /// <summary>
        ///     Builds the request message.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private static HttpRequestMessage BuildMessage(RequestData _) => new(HttpMethod.Get, EndpointUrl);

        /// <inheritdoc />
        public (MetadataResponse, HttpResponseMessage) Send(HttpClient httpClient, RequestData requestData)
        {
            var request = BuildMessage(requestData);
            var response = httpClient.Send(request);
            return (response.Content.ReadFromJsonAsync<MetadataResponse>().Result, response);

        }

        /// <inheritdoc />
        public Task<(MetadataResponse, HttpResponseMessage)> SendAsync(HttpClient httpClient, RequestData requestData)
        {
            var request = BuildMessage(requestData);
            var response = httpClient.SendAsync(request);
            return Task.FromResult((response.Result.Content.ReadFromJsonAsync<MetadataResponse>().Result, response.Result));
        }

    }
}