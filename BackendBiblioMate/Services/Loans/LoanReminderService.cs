using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Notifications;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Service responsible for analyzing loan records and sending user notifications
    /// about upcoming due dates and overdue items.
    /// </summary>
    /// <remarks>
    /// This service is typically invoked on a scheduled basis by
    /// <see cref="LoanReminderBackgroundService"/>.
    /// </remarks>
    public class LoanReminderService
    {
        private readonly BiblioMateDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly INotificationLogService _logService;

        /// <summary>
        /// Number of hours before the due date to trigger a reminder notification.
        /// </summary>
        private const int ReminderWindowHours = 24;

        /// <summary>
        /// Initializes a new instance of <see cref="LoanReminderService"/>.
        /// </summary>
        /// <param name="context">EF Core database context for accessing loan and user data.</param>
        /// <param name="notificationService">Service for sending notifications to users.</param>
        /// <param name="logService">Service for persisting notification log entries.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
        public LoanReminderService(
            BiblioMateDbContext context,
            NotificationService notificationService,
            INotificationLogService logService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// Sends reminder notifications to users with loans that are due
        /// within the <see cref="ReminderWindowHours"/> window.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        public async Task SendReturnRemindersAsync(CancellationToken cancellationToken = default)
        {
            var currentUtc = DateTime.UtcNow;
            var reminderThreshold = currentUtc.AddHours(ReminderWindowHours);

            // Fetch unreturned loans whose due date falls within the reminder window
            var upcomingLoans = await GetUnreturnedLoans()
                .Where(l => l.DueDate >= currentUtc && l.DueDate <= reminderThreshold)
                .ToListAsync(cancellationToken);

            foreach (var loan in upcomingLoans)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var hoursLeft = Math.Ceiling((loan.DueDate - currentUtc).TotalHours);
                var notificationMessage =
                    $"Reminder: '{loan.Book.Title}' is due in {hoursLeft}h (due at {loan.DueDate:yyyy-MM-dd HH:mm} UTC).";

                await SendNotificationAndLogAsync(
                    loan.UserId,
                    NotificationType.ReturnReminder,
                    notificationMessage,
                    cancellationToken);
            }
        }

        /// <summary>
        /// Sends overdue notifications to users with loans that are past their due date.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        public async Task SendOverdueNotificationsAsync(CancellationToken cancellationToken = default)
        {
            var currentUtc = DateTime.UtcNow;

            // Fetch all unreturned loans with due date already passed
            var overdueLoans = await GetUnreturnedLoans()
                .Where(l => l.DueDate < currentUtc)
                .ToListAsync(cancellationToken);

            foreach (var loan in overdueLoans)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var daysLate = Math.Max(1, (currentUtc - loan.DueDate).Days);
                var notificationMessage =
                    $"Overdue: '{loan.Book.Title}' is {daysLate} day(s) late. Please return it as soon as possible.";

                await SendNotificationAndLogAsync(
                    loan.UserId,
                    NotificationType.OverdueNotice,
                    notificationMessage,
                    cancellationToken);
            }
        }

        /// <summary>
        /// Retrieves all active loans that have not yet been returned,
        /// including the associated <see cref="User"/> and <see cref="Book"/>.
        /// </summary>
        /// <returns>Queryable for further filtering.</returns>
        private IQueryable<Loan> GetUnreturnedLoans()
        {
            return _context.Loans
                .Where(l => l.ReturnDate == null)
                .Include(l => l.User)
                .Include(l => l.Book);
        }

        /// <summary>
        /// Sends a notification to the specified user and writes a corresponding log entry.
        /// </summary>
        /// <param name="userId">User identifier to notify.</param>
        /// <param name="type">Classification of the notification.</param>
        /// <param name="message">Content of the notification.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        private async Task SendNotificationAndLogAsync(
            int userId,
            NotificationType type,
            string message,
            CancellationToken cancellationToken)
        {
            await _notificationService.NotifyUser(userId, message, cancellationToken);
            await _logService.LogAsync(userId, type, message, cancellationToken);
        }
    }
}
