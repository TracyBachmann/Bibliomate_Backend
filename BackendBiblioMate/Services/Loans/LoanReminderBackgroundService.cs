namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Background service that runs on a fixed interval to send loan return reminders
    /// and overdue notifications to users with active loans.
    /// </summary>
    /// <remarks>
    /// This service creates a new scoped DI context on each run,
    /// ensuring proper disposal of scoped dependencies such as DbContext.
    /// </remarks>
    public class LoanReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoanReminderBackgroundService> _logger;

        /// <summary>
        /// Defines the interval between reminder executions.
        /// </summary>
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        /// <summary>
        /// Initializes a new instance of <see cref="LoanReminderBackgroundService"/>.
        /// </summary>
        /// <param name="serviceProvider">The root DI container used to create scoped services.</param>
        /// <param name="logger">Logger for operational messages and error tracking.</param>
        /// <exception cref="ArgumentNullException">Thrown if dependencies are not provided.</exception>
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
        /// Executes the background service loop. On each iteration:
        /// <list type="bullet">
        /// <item>Creates a scoped <see cref="LoanReminderService"/>.</item>
        /// <item>Sends loan return reminders.</item>
        /// <item>Sends overdue notifications.</item>
        /// <item>Waits <see cref="Interval"/> before repeating, until cancellation.</item>
        /// </list>
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "LoanReminderBackgroundService started. Interval = {Interval}", 
                Interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a scope for scoped dependencies (DbContext, services, etc.)
                    using var scope = _serviceProvider.CreateScope();
                    var reminderService = scope.ServiceProvider
                                               .GetRequiredService<LoanReminderService>();

                    _logger.LogInformation("Processing loan reminders and overdue notifications...");
                    await reminderService.SendReturnRemindersAsync(stoppingToken);
                    await reminderService.SendOverdueNotificationsAsync(stoppingToken);

                    _logger.LogInformation("Loan reminder cycle completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing loan reminders.");
                }

                try
                {
                    // Wait for next interval, but allow early exit if cancellation requested
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected on shutdown — safe to ignore
                }
            }

            _logger.LogInformation("LoanReminderBackgroundService stopped.");
        }
    }
}
