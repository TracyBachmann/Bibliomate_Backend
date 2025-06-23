namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving notification information.
    /// </summary>
    public class NotificationReadDto
    {
        /// <summary>
        /// Unique identifier of the notification.
        /// </summary>
        /// <example>10</example>
        public int NotificationId { get; set; }

        /// <summary>
        /// Identifier of the user who received the notification.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user who received the notification.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Title of the notification.
        /// </summary>
        /// <example>Overdue Book Reminder</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Body message of the notification.
        /// </summary>
        /// <example>Your loan for "The Hobbit" is overdue by 3 days.</example>
        public string Message { get; set; } = string.Empty;
    }
}