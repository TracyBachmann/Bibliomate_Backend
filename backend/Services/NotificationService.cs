using Microsoft.AspNetCore.SignalR;
using backend.Data;
using backend.Hubs;

namespace backend.Services
{
    /// <summary>
    /// Handles sending notifications to users both in real-time via SignalR and by email.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly SendGridEmailService         _emailService;
        private readonly BiblioMateDbContext          _context;

        /// <summary>
        /// Constructs a new <see cref="NotificationService"/>.
        /// </summary>
        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            SendGridEmailService         emailService,
            BiblioMateDbContext          context)
        {
            _hubContext   = hubContext;
            _emailService = emailService;
            _context      = context;
        }

        /// <inheritdoc/>
        public async Task NotifyUser(int userId, string message)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // 1) Real-time push via SignalR
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", message);

            // 2) Email notification via SendGrid
            await _emailService.SendEmailAsync(
                toEmail:     user.Email,
                subject:     "BiblioMate Notification",
                htmlContent: message);
        }
    }
}