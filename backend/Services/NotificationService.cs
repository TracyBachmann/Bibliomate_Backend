using Microsoft.AspNetCore.SignalR;
using backend.Hubs;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly SendGridEmailService _emailService;
        private readonly BiblioMateDbContext _context;

        public NotificationService(IHubContext<NotificationHub> hubContext, SendGridEmailService emailService, BiblioMateDbContext context)
        {
            _hubContext = hubContext;
            _emailService = emailService;
            _context = context;
        }

        public async Task NotifyUser(int userId, string message)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Envoi SignalR
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", message);

            // Envoi Email
            await _emailService.SendEmailAsync(user.Email, "Notification BiblioMate", message);
        }
    }
}