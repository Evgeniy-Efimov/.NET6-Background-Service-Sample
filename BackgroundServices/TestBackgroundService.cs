using BackgroundService.BackgroundServices.BaseImplementation;
using BackgroundService.Configuration;
using BackgroundService.Http.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundService.BackgroundServices
{
    public class TestBackgroundService : BackgroundServiceBase<TestBackgroundService>
    {
        private readonly IHttpClientProvider _httpClientProvider;

        public TestBackgroundService(
            ILogger<TestBackgroundService> logger,
            IOptions<BackgroundServiceSettings> serviceSettings,
            IHttpClientProvider httpClientProvider) : base(logger, serviceSettings)
        {
            _httpClientProvider = httpClientProvider;
        }

        protected override async Task DoWork(CancellationToken cancellationToken)
        {
            var location = "32,81";
            var weatherForecast = await _httpClientProvider.Get<WeatherForecast>($"https://api.weather.gov/gridpoints/TOP/{location}/forecast",
                cancellationToken, headers: new Dictionary<string, string>() { { "User-Agent", "Testing API Client" } });

            _logger.LogInformation($"Weather forecast for {location}:\r\n    " +
                $"{string.Join("\r\n    ", weatherForecast.Properties.Periods.Select(f => $"{f.Name}: {f.Temperature}{f.TemperatureUnit}; {f.WindSpeed} ({f.WindDirection})"))}");
        }
    }

    public class WeatherForecast
    {
        public WeatherForecastProperties Properties { get; set; }
    }

    public class WeatherForecastProperties
    {
        public IEnumerable<WeatherForecastPeriod> Periods { get; set; }
    }

    public class WeatherForecastPeriod
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public int Temperature { get; set; }
        public string TemperatureUnit { get; set; }
        public string WindSpeed { get; set; }
        public string WindDirection { get; set; }
    }
}
