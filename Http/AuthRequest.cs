using BackgroundService.Configuration;
using BackgroundService.Http.Interfaces;

namespace BackgroundService.Http
{
    public class AuthRequest : IAuthRequest
    {
        public object GetAuthCredentials(AuthSettings authSettings)
        {
            return authSettings.AuthParameters;
        }

        public object GetRefreshTokenCredentials(AuthSettings authSettings, string refreshToken)
        {
            var refreshTokenCredentials = authSettings.AuthParameters;

            if (refreshTokenCredentials.ContainsKey("refresh_token"))
            {
                refreshTokenCredentials["refresh_token"] = refreshToken;
            }
            else
            {
                refreshTokenCredentials.Add("refresh_token", refreshToken);
            }

            return refreshTokenCredentials;
        }
    }
}
