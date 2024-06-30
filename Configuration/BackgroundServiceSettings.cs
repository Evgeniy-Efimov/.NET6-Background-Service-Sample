namespace BackgroundService.Configuration
{
    public class BackgroundServiceSettings
    {
        public bool? IsEnabled { get; set; }
        public string ScheduleUTC { get; set; }
        public bool IsIncludeSeconds { get; set; }
        public bool IsRunOnStart { get; set; }
    }
}
