using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// High-level service contract for logging and retrieving notification events.
    /// This service abstracts persistence details and ensures that all notification-related
    /// events are recorded consistently.
    /// </summary>
    public interface INotificationLogService
    {
        /// <summary>
        /// Persists a new notification log entry for a user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user who received the notification. 
        /// This must correspond to an existing user in the system.
        /// </param>
        /// <param name="type">
        /// The type of notification event (e.g. <see cref="NotificationType.Email"/>, 
        /// <see cref="NotificationType.Sms"/>, <see cref="NotificationType.InApp"/>).
        /// </param>
        /// <param name="message">
        /// The content of the notification message to store in the log.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to observe cancellation requests during the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes once the log entry has been persisted.
        /// </returns>
        Task LogAsync(
            int userId,
            NotificationType type,
            string message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all notification logs associated with a specific user,
        /// ordered by <see cref="NotificationLogDocument.SentAt"/> in descending order (newest first).
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose logs should be retrieved.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to observe cancellation requests during the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a 
        /// <see cref="List{NotificationLogDocument}"/> containing the user’s notification history.
        /// The list will be empty if no logs exist for the user.
        /// </returns>
        Task<List<NotificationLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
