namespace BackgroundService.Http.Interfaces
{
    public interface IAuthResponse
    {
        public string GetAccessToken();
        public void SetAccessToken(string token);
        public double? GetAccessTokenExpiresIn();
        public string GetRefreshToken();
        public void SetRefreshToken(string token);
        public double? GetRefreshTokenExpiresIn();
    }
}
