using System.ComponentModel.DataAnnotations;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new notification log entry.
    /// Contains all necessary information to log a notification dispatch.
    /// </summary>
    public class NotificationLogCreateDto
    {
        /// <summary>
        /// Gets or sets the identifier of the user who received the notification.
        /// </summary>
        /// <example>123</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the type of the notification.
        /// </summary>
        /// <remarks>
        /// Common values include:
        /// - <c>LoanReminder</c>  
        /// - <c>ReservationAvailable</c>  
        /// - <c>OverdueAlert</c>  
        /// </remarks>
        /// <example>LoanReminder</example>
        [Required(ErrorMessage = "Type is required.")]
        public NotificationType Type { get; init; }

        /// <summary>
        /// Gets or sets the content of the notification message.
        /// </summary>
        /// <example>Your loan for “The Hobbit” is due tomorrow.</example>
        [Required(ErrorMessage = "Message is required.")]
        [MinLength(1, ErrorMessage = "Message must not be empty.")]
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification was sent.
        /// </summary>
        /// <remarks>
        /// Defaults to the current UTC time if not explicitly provided.
        /// </remarks>
        /// <example>2025-06-30T14:25:00Z</example>
        public DateTime SentAt { get; init; } = DateTime.UtcNow;
    }
}