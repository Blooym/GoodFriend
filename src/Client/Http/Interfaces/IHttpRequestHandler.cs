using System.Net.Http;
using System.Threading.Tasks;

namespace GoodFriend.Client.Http.Interfaces;

public interface IHttpRequestHandler<TRequestData, TResponse> where TResponse : notnull
{
    public TResponse Send(HttpClient httpClient, TRequestData requestData);
    public Task<TResponse> SendAsync(HttpClient httpClient, TRequestData requestData);
}
