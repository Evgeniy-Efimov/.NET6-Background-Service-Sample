using BackgroundService.Helpers;
using BackgroundService.Http.Interfaces;
using Newtonsoft.Json;

namespace BackgroundService.Http
{
    public class AuthResponse : IAuthResponse
    {
        [JsonProperty("access_token")]
        public string? Token { get; set; }

        [JsonProperty("expires_in")]
        public double? ExpiresIn { get; set; } = SettingsHelper.HttpClientSettings.Auth.DefaultTokenExpiresIn;

        [JsonProperty("refresh_expires_in")]
        public double? RefreshExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string? TokenType { get; set; }

        [JsonProperty("id_token")]
        public string? IdToken { get; set; }

        [JsonProperty("not-before-policy")]
        public int? NotBeforePolicy { get; set; }

        [JsonProperty("session_state")]
        public string? SessionState { get; set; }

        [JsonProperty("scope")]
        public string? Scope { get; set; }

        public double? GetAccessTokenExpiresIn()
        {
            return ExpiresIn;
        }

        public string GetAccessToken()
        {
            return Token;
        }

        public void SetAccessToken(string token)
        {
            Token = token;
        }

        public double? GetRefreshTokenExpiresIn()
        {
            return RefreshExpiresIn;
        }

        public string GetRefreshToken()
        {
            return RefreshToken;
        }

        public void SetRefreshToken(string token)
        {
            RefreshToken = token;
        }
    }
}
