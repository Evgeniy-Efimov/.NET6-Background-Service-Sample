using BackgroundService.Helpers;
using System.Net;

namespace BackgroundService.Enums
{
    public static class SecurityProtocolTypes
    {
        public static readonly Dictionary<string, SecurityProtocolType> SecurityProtocolMapping = new Dictionary<string, SecurityProtocolType>()
        {
            { "SystemDefault", SecurityProtocolType.SystemDefault },
            { "Ssl3", SecurityProtocolType.Ssl3 },
            { "Tls", SecurityProtocolType.Tls },
            { "Tls11", SecurityProtocolType.Tls11 },
            { "Tls12", SecurityProtocolType.Tls12 },
            { "Tls13", SecurityProtocolType.Tls13 }
        };

        public static SecurityProtocolType GetSecurityProtocolType(IEnumerable<string> protocolNames)
        {
            var result = SecurityProtocolType.SystemDefault;

            foreach (var protocolName in protocolNames.Distinct())
            {
                if (SecurityProtocolMapping.Any(m => m.Key.NormalizeString() == protocolName.NormalizeString()))
                {
                    result = result | SecurityProtocolMapping.First(m => m.Key.NormalizeString() == protocolName.NormalizeString()).Value;
                }
            }

            return result;
        }
    }
}
