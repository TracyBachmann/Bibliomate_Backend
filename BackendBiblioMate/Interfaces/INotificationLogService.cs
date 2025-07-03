using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines contract for logging and retrieving notification events.
    /// </summary>
    public interface INotificationLogService
    {
        /// <summary>
        /// Inserts a new notification log entry.
        /// </summary>
        /// <param name="userId">The identifier of the user who received the notification.</param>
        /// <param name="type">The type of notification event.</param>
        /// <param name="message">The content of the notification message.</param>
        /// <param name="cancellationToken">Token to observe cancellation.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the log entry has been persisted.
        /// </returns>
        Task LogAsync(
            int userId,
            NotificationType type,
            string message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all notification logs for a given user, sorted most recent first.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs are to be retrieved.</param>
        /// <param name="cancellationToken">Token to observe cancellation.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a
        /// <see cref="List{NotificationLogDocument}"/> containing the user's notification history.
        /// </returns>
        Task<List<NotificationLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}