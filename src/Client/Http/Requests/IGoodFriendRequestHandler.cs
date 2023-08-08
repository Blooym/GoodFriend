using System.Net.Http;
using System.Threading.Tasks;

namespace GoodFriend.Client.Http.Requests
{
    public interface IGoodFriendRequestHandler<TRequestData, TResponse> where TResponse : notnull
    {
        public TResponse Send(HttpClient httpClient, TRequestData requestData);
        public Task<TResponse> SendAsync(HttpClient httpClient, TRequestData requestData);
    }
}
