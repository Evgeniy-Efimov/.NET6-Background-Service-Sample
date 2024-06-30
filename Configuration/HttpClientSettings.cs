namespace BackgroundService.Configuration
{
    public class HttpClientSettings
    {
        public string BaseAddress { get; set; }
        public bool? IsUseProxy { get; set; }
        public string? Proxy { get; set; }
        public AuthSettings Auth { get; set; }
    }

    public class AuthSettings
    {
        public string AuthType { get; set; }
        public string AuthUrl { get; set; }
        public string RefreshTokenUrl { get; set; }
        public string HttpMethod { get; set; }
        public string ContentType { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public DateTime? TokenExpiresUTC { get; set; }
        public double? DefaultTokenExpiresIn { get; set; }
        public bool? IsUseRefreshToken { get; set; } = false;
        public double? RefreshTokenExpiringDeltaInSeconds { get; set; } = 0;
        public IDictionary<string, string> AuthParameters { get; set; }
        public IDictionary<string, string> RefreshTokenParameters { get; set; }
    }
}
