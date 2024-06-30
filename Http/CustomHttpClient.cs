using BackgroundService.Enums;
using BackgroundService.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BackgroundService.Http
{
    public class CustomHttpClient
    {
        private WebProxy proxy;
        private HttpClientHandler handler;
        public HttpClient client;

        public CustomHttpClient(string proxyUrl = null, bool useProxy = false, bool allowAutoRedirect = false)
        {
            if (useProxy && !string.IsNullOrWhiteSpace(proxyUrl))
            {
                proxy = new WebProxy(proxyUrl, false);
                handler = new HttpClientHandler()
                {
                    Proxy = proxy,
                    UseProxy = true
                };
            }
            else
            {
                handler = new HttpClientHandler();
            }

            handler.AllowAutoRedirect = allowAutoRedirect;
            client = new HttpClient(handler);
        }

        public static HttpClient GetClient(string baseAddress = "", string authType = "", bool useProxy = false, string proxyUrl = "",
            bool allowAutoRedirect = false, string login = "", string password = "", string token = "")
        {
            var client = new CustomHttpClient(proxyUrl, useProxy, allowAutoRedirect).client;

            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                client.BaseAddress = new Uri(baseAddress);
            }

            if (client.DefaultRequestHeaders.Contains("Authorization"))
                client.DefaultRequestHeaders.Remove("Authorization");

            if (client.DefaultRequestHeaders.Contains("api-key"))
                client.DefaultRequestHeaders.Remove("api-key");

            if (authType.NormalizeString() == AuthTypes.Basic.NormalizeString())
            {
                var authByteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", login, password));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authByteArray));
            }
            else if (authType.NormalizeString() == AuthTypes.OAuth.NormalizeString())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else if (authType.NormalizeString() == AuthTypes.ApiKey.NormalizeString())
            {
                client.DefaultRequestHeaders.Add("api-key", token);
            }

            return client;
        }
    }
}
