using BackendBiblioMate.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to create a new notification log entry.
    /// Contains all necessary information to log a notification.
    /// </summary>
    public class NotificationLogCreateDto
    {
        /// <summary>
        /// Gets the identifier of the user who received the notification.
        /// </summary>
        /// <example>123</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets the type of notification.
        /// </summary>
        /// <remarks>
        /// Examples: LoanReminder, ReservationAvailable, OverdueAlert.
        /// </remarks>
        /// <example>LoanReminder</example>
        [Required(ErrorMessage = "Type is required.")]
        public NotificationType Type { get; init; }

        /// <summary>
        /// Gets the content of the notification message.
        /// </summary>
        /// <example>Your loan for “The Hobbit” is due tomorrow.</example>
        [Required(ErrorMessage = "Message is required.")]
        [MinLength(1, ErrorMessage = "Message cannot be empty.")]
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Gets the UTC timestamp when the notification was sent.
        /// </summary>
        /// <remarks>
        /// Defaults to the current UTC time if not provided.
        /// </remarks>
        /// <example>2025-06-30T14:25:00Z</example>
        public DateTime SentAt { get; init; } = DateTime.UtcNow;
    }
}