using CapstoneProject.Application.Interface.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CapstoneProject.Infrastructure.Services
{


    public class ReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderWorker> _logger;

        public ReminderWorker(IServiceProvider serviceProvider, ILogger<ReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var evalService = scope.ServiceProvider.GetRequiredService<IEvaluationService>();
                        await evalService.CheckAndSendRemindersAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing reminder tasks.");
                }

                // Chờ 15-20 phút trước khi quét lại lần nữa
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}
