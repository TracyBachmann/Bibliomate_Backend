namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Hosted background service that periodically triggers
    /// sending of loan return reminders and overdue notifications.
    /// </summary>
    public class LoanReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoanReminderBackgroundService> _logger;

        /// <summary>
        /// Interval between reminder runs.
        /// </summary>
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        /// <summary>
        /// Initializes a new instance of <see cref="LoanReminderBackgroundService"/>.
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
            _serviceProvider = serviceProvider 
                ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger 
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        /// <summary>
        /// Core execution loop. Runs once per <see cref="Interval"/> until cancellation.
        /// </summary>
        /// <param name="stoppingToken">
        /// Token that signals shutdown of the background service.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the service stops.
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "LoanReminderBackgroundService started. Interval = {Interval}.", 
                Interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a DI scope to resolve scoped services safely
                    using var scope = _serviceProvider.CreateScope();
                    var reminderService = scope.ServiceProvider
                                               .GetRequiredService<LoanReminderService>();

                    _logger.LogInformation("Sending return reminders and overdue notifications...");
                    await reminderService.SendReturnRemindersAsync(stoppingToken);
                    await reminderService.SendOverdueNotificationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while sending loan reminders/notifications.");
                }

                try
                {
                    // Delay for configured interval or until cancellation
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected on shutdown; swallow to allow loop exit
                }
            }

            _logger.LogInformation("LoanReminderBackgroundService stopped.");
        }
    }
}