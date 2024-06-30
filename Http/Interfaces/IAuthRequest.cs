using BackgroundService.Configuration;

namespace BackgroundService.Http.Interfaces
{
    public interface IAuthRequest
    {
        public object GetAuthCredentials(AuthSettings authSettings);
        public object GetRefreshTokenCredentials(AuthSettings authSettings, string refreshToken);
    }
}
