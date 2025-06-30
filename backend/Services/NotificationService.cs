using Microsoft.AspNetCore.SignalR;
using backend.Data;
using backend.Hubs;

namespace backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService                _emailService;
        private readonly BiblioMateDbContext          _context;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            IEmailService                emailService,
            BiblioMateDbContext          context)
        {
            _hubContext   = hubContext;
            _emailService = emailService;
            _context      = context;
        }

        public async Task NotifyUser(int userId, string message)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", message);

            await _emailService.SendEmailAsync(
                toEmail:     user.Email,
                subject:     "BiblioMate Notification",
                htmlContent: message);
        }
    }
}