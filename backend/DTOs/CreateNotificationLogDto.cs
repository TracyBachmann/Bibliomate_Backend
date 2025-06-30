using backend.Models.Enums;

namespace backend.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new notification log entry.
    /// </summary>
    public class CreateNotificationLogDto
    {
        /// <summary>
        /// Gets or sets the identifier of the user who received the notification.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the type of notification (e.g. LoanReminder, ReservationAvailable).
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets the content of the notification message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification was sent.
        /// Defaults to <see cref="System.DateTime.UtcNow"/>.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}