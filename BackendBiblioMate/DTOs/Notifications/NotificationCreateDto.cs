using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to request the creation of a new notification.
    /// Contains the details needed to send a notification to a user.
    /// </summary>
    public class NotificationCreateDto
    {
        /// <summary>
        /// Gets or sets the identifier of the user who will receive the notification.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        /// <remarks>
        /// Must contain between 1 and 200 characters.
        /// </remarks>
        /// <example>Overdue Book Reminder</example>
        [Required(ErrorMessage = "Title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the body message of the notification.
        /// </summary>
        /// <remarks>
        /// Must contain between 1 and 1000 characters.
        /// </remarks>
        /// <example>Your loan for “The Hobbit” is overdue by 3 days.</example>
        [Required(ErrorMessage = "Message is required.")]
        [MinLength(1, ErrorMessage = "Message must be at least 1 character long.")]
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; init; } = string.Empty;
    }
}