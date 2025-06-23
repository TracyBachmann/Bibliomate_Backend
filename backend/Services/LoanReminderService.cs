using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Service to send reminders and overdue notifications for book loans.
    /// </summary>
    public class LoanReminderService
    {
        private readonly BiblioMateDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly NotificationLogService _logService;

        public LoanReminderService(
            BiblioMateDbContext context,
            NotificationService notificationService,
            NotificationLogService logService)
        {
            _context            = context;
            _notificationService = notificationService;
            _logService         = logService;
        }

        /// <summary>
        /// Sends reminders for loans due in the next 24 hours.
        /// </summary>
        public async Task SendReturnRemindersAsync()
        {
            var now               = DateTime.UtcNow;
            var upcomingThreshold = now.AddHours(24);

            var upcomingLoans = await _context.Loans
                .Where(l => l.ReturnDate == null
                            && l.DueDate >= now
                            && l.DueDate <= upcomingThreshold)
                .Include(l => l.User)
                .Include(l => l.Book)
                .ToListAsync();

            foreach (var loan in upcomingLoans)
            {
                var hoursLeft = Math.Ceiling((loan.DueDate - now).TotalHours);
                var message   = $"Rappel : votre emprunt du livre « {loan.Book.Title} » est dû dans {hoursLeft} heure(s) (retour prévu : {loan.DueDate:yyyy-MM-dd HH:mm}).";

                // 1) Send
                await _notificationService.NotifyUser(loan.UserId, message);

                // 2) Log
                await _logService.LogAsync(
                    loan.UserId,
                    NotificationType.ReturnReminder,
                    message
                );
            }
        }

        /// <summary>
        /// Sends notifications for overdue loans.
        /// </summary>
        public async Task SendOverdueNotificationsAsync()
        {
            var now = DateTime.UtcNow;

            var overdueLoans = await _context.Loans
                .Where(l => l.ReturnDate == null
                            && l.DueDate < now)
                .Include(l => l.User)
                .Include(l => l.Book)
                .ToListAsync();

            foreach (var loan in overdueLoans)
            {
                var daysLate = (now - loan.DueDate).Days;
                var message  = $"Notification de retard : votre emprunt du livre « {loan.Book.Title} » a {daysLate} jour(s) de retard. Merci de le retourner rapidement.";

                await _notificationService.NotifyUser(loan.UserId, message);

                await _logService.LogAsync(
                    loan.UserId,
                    NotificationType.OverdueNotice,
                    message
                );
            }
        }
    }
}
