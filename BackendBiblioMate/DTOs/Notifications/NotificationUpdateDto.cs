using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update an existing notification.
    /// Contains fields that can be modified on a notification record.
    /// </summary>
    public class NotificationUpdateDto
    {
        /// <summary>
        /// Gets the unique identifier of the notification to update.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "NotificationId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "NotificationId must be a positive integer.")]
        public int NotificationId { get; init; }

        /// <summary>
        /// Gets the identifier of the user who will receive the notification.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets the updated title of the notification.
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>Overdue Book Reminder</example>
        [Required(ErrorMessage = "Title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated body message of the notification.
        /// </summary>
        /// <remarks>
        /// Maximum length of 1000 characters.
        /// </remarks>
        /// <example>Your loan for “The Hobbit” is overdue by 3 days.</example>
        [Required(ErrorMessage = "Message is required.")]
        [MinLength(1, ErrorMessage = "Message must be at least 1 character long.")]
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; init; } = string.Empty;
    }
}