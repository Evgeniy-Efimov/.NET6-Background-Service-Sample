using BackgroundService.Configuration;
using BackgroundService.Enums;
using BackgroundService.Helpers;
using BackgroundService.Http.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace BackgroundService.Http
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly ILogger<HttpClientProvider> _logger;
        private AuthSettings AuthSettings { get; set; }
        private HttpClientSettings HttpClientSettings { get; set; }
        private string BaseAddress { get; set; }
        private string AuthUrl { get; set; }
        private string RefreshTokenUrl { get; set; }
        private static AccessToken AccessToken { get; set; } = new AccessToken(null, null);
        private static AccessToken RefreshToken { get; set; } = new AccessToken(null, null);

        private static readonly object _authLocker = new object();
        private static bool _isAuthProcessing = false;

        public HttpClientProvider(ILogger<HttpClientProvider> logger,
            IOptions<HttpClientSettings> httpClientSettings)
        {
            _logger = logger;
            AuthSettings = httpClientSettings.Value.Auth;
            HttpClientSettings = httpClientSettings.Value;
            BaseAddress = httpClientSettings.Value.BaseAddress;
            AuthUrl = httpClientSettings.Value.Auth.AuthUrl;
            RefreshTokenUrl = httpClientSettings.Value.Auth.RefreshTokenUrl;
            AccessToken = new AccessToken(AuthSettings.Token, AuthSettings.TokenExpiresUTC);
        }

        public async Task<TResponse> Delete<TResponse>(string url, CancellationToken cancellationToken, IDictionary<string, string> headers = null)
        {
            return await Request<TResponse, object>(url, cancellationToken, contentObject: null, HttpMethod.Delete, headers: headers);
        }

        public async Task<TResponse> Get<TResponse>(string url, CancellationToken cancellationToken, IDictionary<string, string> headers = null)
        {
            return await Request<TResponse, object>(url, cancellationToken, contentObject: null, HttpMethod.Get, headers: headers);
        }

        public async Task<TResponse> Post<TResponse, TRequest>(string url, CancellationToken cancellationToken,
            TRequest contentObject = null, string contentType = "", IDictionary<string, string> headers = null) where TRequest : class
        {
            return await Request<TResponse, TRequest>(url, cancellationToken, contentObject, HttpMethod.Post, contentType, headers: headers);
        }

        public async Task<TResponse> Put<TResponse, TRequest>(string url, CancellationToken cancellationToken,
            TRequest contentObject = null, string contentType = "", IDictionary<string, string> headers = null) where TRequest : class
        {
            return await Request<TResponse, TRequest>(url, cancellationToken, contentObject, HttpMethod.Put, contentType, headers: headers);
        }

        private async Task<TResponse> Request<TResponse, TRequest>(string url, CancellationToken cancellationToken,
            TRequest contentObject, HttpMethod methodType, string contentType = null, IDictionary<string, string> headers = null,
            string authType = null, bool isAuthRequest = false) where TRequest : class
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Request url is empty");

            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = HttpContentTypes.UrlEncoded;
            }

            if (string.IsNullOrWhiteSpace(authType))
            {
                authType = AuthSettings.AuthType;
            }

            var urlType = url.StartsWith("http") ? UriKind.Absolute : UriKind.Relative;

            var request = new HttpRequestMessage
            {
                Method = methodType,
                RequestUri = new Uri(url, urlType)
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (contentType == HttpContentTypes.Json)
            {
                if (contentObject != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(contentObject));
                }
                else
                {
                    request.Content = new StringContent("");
                }
            }
            else if (contentType == HttpContentTypes.UrlEncoded && contentObject != null)
            {
                var propertyValueDictionary = contentObject as IDictionary<string, string>;

                if (propertyValueDictionary != null)
                {
                    request.Content = new FormUrlEncodedContent(propertyValueDictionary);
                }
                else
                {
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>() { });
                }
            }

            if (request.Content != null)
            {
                request.Content.Headers.Clear();
                request.Content.Headers.Add("Content-Type", contentType);
            }

            if (IsUpdateAccessToken() && !isAuthRequest)
            {
                UpdateAccessToken(cancellationToken);
            }

            var client = CustomHttpClient.GetClient(
                baseAddress: urlType == UriKind.Relative ? BaseAddress : null,
                authType: authType,
                useProxy: HttpClientSettings.IsUseProxy ?? false,
                proxyUrl: HttpClientSettings.Proxy,
                login: AuthSettings.Login,
                password: AuthSettings.Password,
                token: AccessToken.GetToken());

            var response = await client.SendAsync(request, cancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (typeof(TResponse) == typeof(string))
                    return (TResponse)(object)responseContent;

                return await Task.Run(() => ReadResponseAs<TResponse>(responseContent));
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized
                    && AuthSettings.AuthType.NormalizeString() == AuthTypes.OAuth.NormalizeString()
                    && !isAuthRequest)
                {
                    ResetAccessToken();
                }

                var errorResponse = ReadResponseAs<ErrorResponse>(responseContent);

                throw new Exception($"Status code: {errorResponse?.GetStatusCode() ?? response.StatusCode.ToString()}; " +
                    $"Reason phrase: {response.ReasonPhrase}; " +
                    $"Message: {errorResponse?.GetErrorMessage() ?? responseContent}");
            }
        }

        private void UpdateAccessToken(CancellationToken cancellationToken)
        {
            while (_isAuthProcessing)
            {
                _logger.LogInformation($"Awaiting auth in {this.GetType().Name} caused by other process...");
                Thread.Sleep(1000);
            }

            try
            {
                lock (_authLocker)
                {
                    _isAuthProcessing = true;
                }

                var timer = new Stopwatch();
                timer.Start();

                AuthResponse authResponse = null;
                string responseString = null;

                if (IsUseRefreshToken())
                {
                    var credentials = new AuthRequest().GetRefreshTokenCredentials(AuthSettings, RefreshToken.GetToken());

                    responseString = Request<string, object>(RefreshTokenUrl, cancellationToken, credentials,
                        new HttpMethod(AuthSettings.HttpMethod?.ToUpper()), AuthSettings.ContentType,
                        authType: AuthTypes.None, isAuthRequest: true).Result;
                }
                else
                {
                    var credentials = new AuthRequest().GetAuthCredentials(AuthSettings);

                    responseString = Request<string, object>(AuthUrl, cancellationToken, credentials,
                        new HttpMethod(AuthSettings.HttpMethod?.ToUpper()), AuthSettings.ContentType,
                        authType: AuthTypes.None, isAuthRequest: true).Result;
                }

                authResponse = ReadResponseAs<AuthResponse>(responseString);

                if (authResponse == null)
                {
                    authResponse = new AuthResponse();
                    authResponse.SetAccessToken(responseString);
                }

                lock (AccessToken)
                {
                    AccessToken.SetToken(authResponse.GetAccessToken(),
                        expiresUTC: authResponse.GetAccessTokenExpiresIn() != null
                            ? DateTime.UtcNow.AddSeconds(authResponse.GetAccessTokenExpiresIn().Value)
                            : null);
                }

                if (AuthSettings.IsUseRefreshToken ?? false)
                {
                    lock (RefreshToken)
                    {
                        RefreshToken.SetToken(authResponse.GetRefreshToken(),
                            expiresUTC: authResponse.GetRefreshTokenExpiresIn() != null
                                ? DateTime.UtcNow.AddSeconds(authResponse.GetRefreshTokenExpiresIn().Value)
                                : null);
                    }
                }

                _logger.LogInformation($"Auth token for {this.GetType().Name} has been updated. Time taken: {timer.Elapsed.ToString("g")}");
                timer.Reset();

                var accessToken = AccessToken.GetToken();

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new Exception($"Auth error, access token is empty");
            }
            catch (Exception)
            {
                lock (_authLocker)
                {
                    _isAuthProcessing = false;
                }

                throw;
            }
            finally
            {
                lock (_authLocker)
                {
                    _isAuthProcessing = false;
                }
            }
        }

        private void ResetAccessToken()
        {
            while (_isAuthProcessing)
            {
                _logger.LogInformation($"Awaiting auth in {this.GetType().Name} caused by other process...");
                Thread.Sleep(1000);
            }

            try
            {
                lock (_authLocker)
                {
                    _isAuthProcessing = true;
                }

                lock (AccessToken)
                {
                    AccessToken.SetToken(null, DateTime.UtcNow);
                }

                _logger.LogInformation($"Invalid access token for {this.GetType().Name} has been reset");
            }
            catch (Exception)
            {
                lock (_authLocker)
                {
                    _isAuthProcessing = false;
                }

                throw;
            }
            finally
            {
                lock (_authLocker)
                {
                    _isAuthProcessing = false;
                }
            }
        }

        private bool IsUpdateAccessToken()
        {
            return AuthSettings.AuthType.NormalizeString() == AuthTypes.OAuth.NormalizeString()
                && (!AccessToken.IsValid() || (AuthSettings.IsUseRefreshToken.Value
                    && RefreshToken.IsExpiring(AuthSettings.RefreshTokenExpiringDeltaInSeconds.Value)));
        }

        private bool IsUseRefreshToken()
        {
            return (AuthSettings.IsUseRefreshToken ?? false) && RefreshToken.IsValid();
        }

        private TResult ReadResponseAs<TResult>(string responseContent)
        {
            try
            {
                return JsonConvert.DeserializeObject<TResult>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Parsing {typeof(TResult).FullName} from response error\r\n");

                return default(TResult);
            }
        }
    }
}
