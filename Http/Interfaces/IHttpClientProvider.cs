namespace BackgroundService.Http.Interfaces
{
    public interface IHttpClientProvider
    {
        Task<TResponse> Get<TResponse>(string url, CancellationToken cancellationToken, IDictionary<string, string> headers = null);

        Task<TResponse> Delete<TResponse>(string url, CancellationToken cancellationToken, IDictionary<string, string> headers = null);

        Task<TResponse> Post<TResponse, TRequest>(string url, CancellationToken cancellationToken,
            TRequest contentObject = null, string contentType = "", IDictionary<string, string> headers = null) where TRequest : class;

        Task<TResponse> Put<TResponse, TRequest>(string url, CancellationToken cancellationToken,
            TRequest contentObject = null, string contentType = "", IDictionary<string, string> headers = null) where TRequest : class;
    }
}
