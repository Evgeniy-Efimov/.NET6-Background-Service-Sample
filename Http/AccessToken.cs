namespace BackgroundService.Http
{
    public class AccessToken
    {
        private string Token { get; set; }
        private DateTime? ExpiresUTC { get; set; } = DateTime.UtcNow;

        public bool IsValid()
        {
            return DateTime.UtcNow <= ExpiresUTC
                && !string.IsNullOrWhiteSpace(Token);
        }

        public bool IsExpiring(double expiringDeltaInSeconds)
        {
            return DateTime.UtcNow > (ExpiresUTC ?? DateTime.UtcNow).AddSeconds(-expiringDeltaInSeconds);
        }

        public string GetToken()
        {
            return Token;
        }

        public void SetToken(string token, DateTime? expiresUTC)
        {
            ExpiresUTC = expiresUTC ?? DateTime.MaxValue;
            Token = token;
        }

        public AccessToken(string token, DateTime? expiresUTC)
        {
            ExpiresUTC = expiresUTC ?? DateTime.MaxValue;
            Token = token;
        }
    }
}
