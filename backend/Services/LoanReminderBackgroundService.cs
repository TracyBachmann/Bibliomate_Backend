using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    /// <summary>
    /// Service hébergé qui exécute périodiquement les rappels de prêts et les notifications de retard.
    /// </summary>
    public class LoanReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoanReminderBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        public LoanReminderBackgroundService(IServiceProvider serviceProvider,
                                             ILogger<LoanReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LoanReminderBackgroundService démarré, intervalle = {Interval}.", Interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var reminderSvc = scope.ServiceProvider.GetRequiredService<LoanReminderService>();

                    _logger.LogInformation("Envoi des rappels de prêts et notifications de retard...");
                    await reminderSvc.SendReturnRemindersAsync();
                    await reminderSvc.SendOverdueNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l’envoi des rappels/notifications de prêts.");
                }

                // Attend l’intervalle ou l’annulation
                await Task.Delay(Interval, stoppingToken);
            }

            _logger.LogInformation("LoanReminderBackgroundService arrêté.");
        }
    }
}
