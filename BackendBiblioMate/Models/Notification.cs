using System;
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
        /// Gets the primary key of the notification.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who will receive the notification.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the body message of the notification.
        /// </summary>
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(255, ErrorMessage = "Message cannot exceed 255 characters.")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification was created.
        /// </summary>
        [Required(ErrorMessage = "Timestamp is required.")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for the user receiving the notification.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}