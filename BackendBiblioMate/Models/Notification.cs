using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a notification sent to a user, either via email or real-time push.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the unique identifier of the notification.
        /// </summary>
        /// <example>10</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who will receive the notification.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>Overdue Book Reminder</example>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the body message of the notification.
        /// </summary>
        /// <remarks>
        /// Maximum length of 1000 characters.
        /// </remarks>
        /// <example>Your loan for “The Hobbit” is overdue by 3 days.</example>
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification was created.
        /// </summary>
        /// <example>2025-07-04T15:30:00Z</example>
        [Required(ErrorMessage = "Timestamp is required.")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the navigation property for the user receiving the notification.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
