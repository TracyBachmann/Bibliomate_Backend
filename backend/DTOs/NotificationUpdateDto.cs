using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing notification.
    /// </summary>
    public class NotificationUpdateDto
    {
        /// <summary>
        /// Unique identifier of the notification to update.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "NotificationId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "NotificationId must be a positive integer.")]
        public int NotificationId { get; set; }

        /// <summary>
        /// Identifier of the user who will receive the notification.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Updated title of the notification.
        /// </summary>
        /// <example>Overdue Book Reminder</example>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated body message of the notification.
        /// </summary>
        /// <example>Your loan for "The Hobbit" is overdue by 3 days.</example>
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}