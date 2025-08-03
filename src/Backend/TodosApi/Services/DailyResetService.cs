using TodosApi.Services;

namespace TodosApi.Services
{
    public class DailyResetService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DailyResetService> _logger;
        private Timer? _timer;

        public DailyResetService(IServiceScopeFactory serviceScopeFactory, ILogger<DailyResetService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Daily Reset Service started.");
            
            // Calculate time until next midnight
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var timeUntilMidnight = nextMidnight - now;

            // Start timer to run at midnight, then every 24 hours
            _timer = new Timer(DoWork, null, timeUntilMidnight, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                _logger.LogInformation("Starting daily reset at {time}", DateTime.Now);

                using var scope = _serviceScopeFactory.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();

                // Save yesterday's statistics before reset
                await historyService.SaveTodayStatisticsAsync();
                
                // Reset all tasks
                await taskService.ResetAllTasksAsync();

                _logger.LogInformation("Daily reset completed successfully at {time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during daily reset");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Daily Reset Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
