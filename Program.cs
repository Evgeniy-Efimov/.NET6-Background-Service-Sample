using BackgroundService.BackgroundServices;
using BackgroundService.Configuration;
using BackgroundService.Helpers;
using BackgroundService.Http;
using BackgroundService.Http.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Net;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        Func<string, IConfigurationSection> getConfigSection = (settingsName) =>
        {
            return context.Configuration.GetSection(settingsName.GetSubstring("Settings"));
        };

        services.Configure<BackgroundServiceSettings>(getConfigSection(nameof(BackgroundServiceSettings)));
        services.Configure<HttpClientSettings>(getConfigSection(nameof(HttpClientSettings)));
        services.Configure<SecuritySettings>(getConfigSection(nameof(SecuritySettings)));

        SettingsHelper.Init(
            getConfigSection(nameof(BackgroundServiceSettings)).Get<BackgroundServiceSettings>(),
            getConfigSection(nameof(HttpClientSettings)).Get<HttpClientSettings>(),
            getConfigSection(nameof(SecuritySettings)).Get<SecuritySettings>());

        services.AddSingleton<IHttpClientProvider, HttpClientProvider>();

        if (SettingsHelper.BackgroundServiceSettings.IsEnabled ?? true)
            services.AddHostedService<TestBackgroundService>();

        ServicePointManager.SecurityProtocol = SettingsHelper.SecurityProtocolType;

        if (!SettingsHelper.SecuritySettings.IsValidateCertificate)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    })
    .ConfigureLogging((context, logging) =>
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        logging.ClearProviders();
        logging.AddSerilog(logger);
    })
    .Build();

await host.RunAsync();