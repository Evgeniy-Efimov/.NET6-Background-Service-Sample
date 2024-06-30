using BackgroundService.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using HostedBackgroundService = Microsoft.Extensions.Hosting.BackgroundService;

namespace BackgroundService.BackgroundServices.BaseImplementation
{
    public abstract class BackgroundServiceBase<TLogger> : HostedBackgroundService
    {
        protected ILogger<TLogger> _logger;
        protected BackgroundServiceSettings _serviceSettings;
        protected CrontabSchedule _processingSchedule;
        protected DateTime _nextProcessing;
        protected bool _isProcessing = false;

        public BackgroundServiceBase(ILogger<TLogger> logger,
            IOptions<BackgroundServiceSettings> serviceSettings)
        {
            _logger = logger;
            _serviceSettings = serviceSettings.Value;

            _processingSchedule = CrontabSchedule.Parse(
                _serviceSettings.ScheduleUTC,
                new CrontabSchedule.ParseOptions
                {
                    IncludingSeconds = _serviceSettings.IsIncludeSeconds
                });

            _nextProcessing = _processingSchedule.GetNextOccurrence(
                _serviceSettings.IsRunOnStart ? DateTime.MinValue : DateTime.UtcNow);
        }

        protected abstract Task DoWork(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Start {this.GetType().Name}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    var millisecondsDelay = GetNextProcessingDelay();

                    LogNextProcessingDateTime(millisecondsDelay);

                    await Task.Delay(millisecondsDelay, cancellationToken);

                    try
                    {
                        try
                        {
                            _isProcessing = true;

                            await DoWork(cancellationToken);

                            _isProcessing = false;
                        }
                        catch (Exception)
                        {
                            _isProcessing = false;

                            throw;
                        }
                        finally
                        {
                            _isProcessing = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{this.GetType().Name} processing error\r\n");
                    }

                    _nextProcessing = _processingSchedule.GetNextOccurrence(DateTime.UtcNow);
                }

                _logger.LogInformation($"Stop {this.GetType().Name}");

                await StopProcessingTasksQueue(cancellationToken);
            }
            catch (Exception ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, $"{this.GetType().Name} execution cancelled\r\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name} unhandled exception\r\n");
            }
        }

        private int GetNextProcessingDelay()
        {
            var millisecondsDelay = _nextProcessing.Subtract(DateTime.UtcNow).TotalMilliseconds;

            return Math.Max(0, millisecondsDelay > int.MaxValue ? int.MaxValue : (int)millisecondsDelay);
        }

        private async Task StopProcessingTasksQueue(CancellationToken token)
        {
            while (_isProcessing)
            {
                await Task.Delay(1000, token);
            }
        }
        private void LogNextProcessingDateTime(int millisecondsDelay)
        {
            if (millisecondsDelay > 0)
            {
                var secondsDelay = Math.Ceiling(millisecondsDelay / 1000d);

                _logger.LogInformation($"Waiting {this.GetType().Name} next processing at " +
                    $"{(DateTime.Now + TimeSpan.FromSeconds(secondsDelay)).ToString("yyyy-MM-dd HH:mm:ss.000 zzz")}");
            }
        }
    }
}
