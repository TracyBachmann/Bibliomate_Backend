namespace backend.Services
{
    /// <summary>
    /// Hosted background service that periodically triggers
    /// sending of loan return reminders and overdue notifications.
    /// </summary>
    public class LoanReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoanReminderBackgroundService> _logger;

        // Interval between reminder runs
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        /// <summary>
        /// Constructs the background service with required dependencies.
        /// </summary>
        /// <param name="serviceProvider">
        /// Service provider used to create scopes for scoped services.
        /// </param>
        /// <param name="logger">
        /// Logger for recording operational messages and errors.
        /// </param>
        public LoanReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<LoanReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        /// <summary>
        /// Core execution loop. Runs once per <see cref="Interval"/> until cancellation.
        /// </summary>
        /// <param name="stoppingToken">Token that signals shutdown.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "LoanReminderBackgroundService started. Interval = {Interval}.", 
                Interval
            );

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new DI scope to get scoped services
                    using var scope = _serviceProvider.CreateScope();
                    var reminderSvc = scope.ServiceProvider
                                           .GetRequiredService<LoanReminderService>();

                    _logger.LogInformation("Sending return reminders and overdue notifications...");
                    await reminderSvc.SendReturnRemindersAsync();
                    await reminderSvc.SendOverdueNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex, 
                        "Error occurred while sending loan reminders/notifications."
                    );
                }

                // Wait for the specified interval or until cancellation
                await Task.Delay(Interval, stoppingToken);
            }

            _logger.LogInformation("LoanReminderBackgroundService stopped.");
        }
    }
}
