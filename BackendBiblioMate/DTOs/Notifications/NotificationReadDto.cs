namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving notification information.
    /// Contains details of a sent notification and its recipient.
    /// </summary>
    public class NotificationReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the notification.
        /// </summary>
        /// <example>10</example>
        public int NotificationId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user who received the notification.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the full name of the user who received the notification.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        /// <example>Overdue Book Reminder</example>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the body message of the notification.
        /// </summary>
        /// <example>Your loan for “The Hobbit” is overdue by 3 days.</example>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification was created.
        /// </summary>
        /// <example>2025-07-04T15:30:00Z</example>
        public DateTime Timestamp { get; init; }
    }
}