using BackgroundService.Configuration;
using BackgroundService.Enums;
using System.Net;

namespace BackgroundService.Helpers
{
    public static class SettingsHelper
    {
        public static BackgroundServiceSettings BackgroundServiceSettings { get; private set; }
        public static HttpClientSettings HttpClientSettings { get; private set; }
        public static SecuritySettings SecuritySettings { get; private set; }

        public static SecurityProtocolType SecurityProtocolType
        {
            get
            {
                return SecurityProtocolTypes.GetSecurityProtocolType(SecuritySettings.SecurityProtocols);
            }
        }

        public static void Init(BackgroundServiceSettings backgroundServiceSettings,
            HttpClientSettings httpClientSettings,
            SecuritySettings securitySettings)
        {
            BackgroundServiceSettings = backgroundServiceSettings;
            HttpClientSettings = httpClientSettings;
            SecuritySettings = securitySettings;
        }
    }
}
