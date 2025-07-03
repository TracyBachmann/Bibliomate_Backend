using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Notifications;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Service responsible for sending loan return reminders and overdue notifications.
    /// </summary>
    public class LoanReminderService
    {
        private readonly BiblioMateDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly INotificationLogService _logService;

        /// <summary>
        /// Number of hours before due date to send a reminder.
        /// </summary>
        private const int ReminderWindowHours = 24;

        /// <summary>
        /// Initializes a new instance of <see cref="LoanReminderService"/>.
        /// </summary>
        /// <param name="context">Database context for accessing loan data.</param>
        /// <param name="notificationService">Service for dispatching user notifications.</param>
        /// <param name="logService">Service for recording notification logs.</param>
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
        /// Sends reminders to users <see cref="ReminderWindowHours"/> hours before their loan due date.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the reminder operation.</returns>
        public async Task SendReturnRemindersAsync(CancellationToken cancellationToken = default)
        {
            var currentUtc = DateTime.UtcNow;
            var reminderThreshold = currentUtc.AddHours(ReminderWindowHours);

            var upcomingLoans = await GetUnreturnedLoans()
                .Where(l => l.DueDate >= currentUtc && l.DueDate <= reminderThreshold)
                .ToListAsync(cancellationToken);

            foreach (var loan in upcomingLoans)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hoursLeft = Math.Ceiling((loan.DueDate - currentUtc).TotalHours);
                var notificationMessage =
                    $"Reminder: '{loan.Book.Title}' is due in {hoursLeft}h (due at {loan.DueDate:yyyy-MM-dd HH:mm}).";

                await SendNotificationAndLogAsync(
                    loan.UserId,
                    NotificationType.ReturnReminder,
                    notificationMessage,
                    cancellationToken);
            }
        }

        /// <summary>
        /// Sends overdue notifications to users with loans past their due date.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the overdue notification operation.</returns>
        public async Task SendOverdueNotificationsAsync(CancellationToken cancellationToken = default)
        {
            var currentUtc = DateTime.UtcNow;

            var overdueLoans = await GetUnreturnedLoans()
                .Where(l => l.DueDate < currentUtc)
                .ToListAsync(cancellationToken);

            foreach (var loan in overdueLoans)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var daysLate = (currentUtc - loan.DueDate).Days;
                var notificationMessage =
                    $"Overdue: '{loan.Book.Title}' is {daysLate} day(s) late.";

                await SendNotificationAndLogAsync(
                    loan.UserId,
                    NotificationType.OverdueNotice,
                    notificationMessage,
                    cancellationToken);
            }
        }

        /// <summary>
        /// Retrieves all loans that have not yet been returned, including associated user and book details.
        /// </summary>
        /// <returns>Queryable collection of unreturned loans with related user and book data.</returns>
        private IQueryable<Loan> GetUnreturnedLoans()
        {
            return _context.Loans
                .Where(l => l.ReturnDate == null)
                .Include(l => l.User)
                .Include(l => l.Book);
        }

        /// <summary>
        /// Dispatches a notification to the user and logs the event.
        /// </summary>
        /// <param name="userId">Identifier of the user to notify.</param>
        /// <param name="type">Type of notification to log.</param>
        /// <param name="message">Content of the notification message.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the send-and-log operation.</returns>
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