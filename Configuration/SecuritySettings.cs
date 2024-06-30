namespace BackgroundService.Configuration
{
    public class SecuritySettings
    {
        public bool IsValidateCertificate { get; set; }
        public string[] SecurityProtocols { get; set; }
    }
}
