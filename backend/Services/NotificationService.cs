using Microsoft.AspNetCore.SignalR;
using backend.Data;
using backend.Hubs;

namespace backend.Services
{
    /// <summary>
    /// Handles sending notifications to users both in real-time via SignalR and by email.
    /// </summary>
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly SendGridEmailService   _emailService;
        private readonly BiblioMateDbContext    _context;

        /// <summary>
        /// Constructs the NotificationService with required dependencies.
        /// </summary>
        /// <param name="hubContext">
        /// SignalR hub context used to push real-time messages to connected clients.
        /// </param>
        /// <param name="emailService">
        /// Service for sending email notifications (SendGrid).
        /// </param>
        /// <param name="context">
        /// EF Core DB context for retrieving user information.
        /// </param>
        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            SendGridEmailService emailService,
            BiblioMateDbContext context)
        {
            _hubContext   = hubContext;
            _emailService = emailService;
            _context      = context;
        }

        /// <summary>
        /// Sends a notification to the specified user.
        /// </summary>
        /// <param name="userId">Identifier of the user to notify.</param>
        /// <param name="message">Notification message content.</param>
        /// <returns>
        /// A task that completes when both real-time and email notifications have been dispatched.
        /// </returns>
        public async Task NotifyUser(int userId, string message)
        {
            // Retrieve the user from the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                // No such user: nothing to do
                return;
            }

            // 1) Real-time push via SignalR
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", message);

            // 2) Email notification via SendGrid
            await _emailService.SendEmailAsync(
                toEmail:    user.Email,
                subject:    "Notification BiblioMate",
                htmlContent: message
            );
        }
    }
}
