using BackendBiblioMate.Data;
using BackendBiblioMate.Hubs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Notifications
{
    /// <summary>
    /// Implements <see cref="INotificationService"/> by sending real-time SignalR notifications
    /// and email messages to users.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="NotificationService"/>.
        /// </summary>
        /// <param name="hubContext">SignalR hub context for pushing real-time notifications.</param>
        /// <param name="emailService">Service responsible for sending emails.</param>
        /// <param name="context">Database context for looking up user information.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            IEmailService emailService,
            BiblioMateDbContext context)
        {
            _hubContext   = hubContext   ?? throw new ArgumentNullException(nameof(hubContext));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _context      = context      ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Sends a notification message to the specified user via SignalR and email.
        /// </summary>
        /// <param name="userId">Identifier of the user to notify.</param>
        /// <param name="message">Notification message content.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the notify operation.</returns>
        public async Task NotifyUser(
            int userId,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message cannot be empty.", nameof(message));

            // 1) Lookup user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (user == null)
                return;

            // 2) Send real-time notification via SignalR
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", message, cancellationToken);

            // 3) Send email notification
            await _emailService.SendEmailAsync(
                toEmail:     user.Email,
                subject:     "BiblioMate Notification",
                htmlContent: message);
        }
    }
}