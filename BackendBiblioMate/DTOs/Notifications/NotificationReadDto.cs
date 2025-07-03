namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving notification information.
    /// Contains details of a sent notification and recipient.
    /// </summary>
    public class NotificationReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the notification.
        /// </summary>
        /// <example>10</example>
        public int NotificationId { get; init; }

        /// <summary>
        /// Gets the identifier of the user who received the notification.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets the full name of the user who received the notification.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the title of the notification.
        /// </summary>
        /// <example>Overdue Book Reminder</example>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the body message of the notification.
        /// </summary>
        /// <example>Your loan for “The Hobbit” is overdue by 3 days.</example>
        public string Message { get; init; } = string.Empty;
    }
}