using BackendBiblioMate.Data;
using BackendBiblioMate.Hubs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Notifications
{
    /// <summary>
    /// Default implementation of <see cref="INotificationService"/>.
    /// Sends notifications to users through real-time SignalR messages
    /// and fallback email delivery.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="hubContext">The SignalR hub context used for pushing real-time notifications.</param>
        /// <param name="emailService">The service responsible for sending email notifications.</param>
        /// <param name="context">The database context used to retrieve user details.</param>
        /// <exception cref="ArgumentNullException">Thrown if any injected dependency is <c>null</c>.</exception>
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
        /// Sends a notification to a user by:
        /// <list type="number">
        ///   <item>Looking up the user in the database.</item>
        ///   <item>Sending a real-time SignalR message if the user is connected.</item>
        ///   <item>Dispatching a fallback email notification.</item>
        /// </list>
        /// </summary>
        /// <param name="userId">The identifier of the user to notify.</param>
        /// <param name="message">The notification message to send.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation of sending the notification.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="message"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        public async Task NotifyUser(
            int userId,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message cannot be empty.", nameof(message));

            // Step 1: Retrieve the target user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (user == null)
                return;

            // Step 2: Send a real-time notification through SignalR
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", message, cancellationToken);

            // Step 3: Send an email notification as a backup
            await _emailService.SendEmailAsync(
                toEmail:     user.Email,
                subject:     "BiblioMate Notification",
                htmlContent: message);
        }
    }
}
