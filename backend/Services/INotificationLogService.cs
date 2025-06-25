using System.Collections.Generic;
using backend.Models.Enums;
using backend.Models.Mongo;

namespace backend.Services
{
    /// <summary>
    /// Defines contract for logging notification events.
    /// </summary>
    public interface INotificationLogService
    {
        /// <summary>
        /// Inserts a new notification log entry.
        /// </summary>
        /// <param name="userId">The identifier of the user who received the notification.</param>
        /// <param name="type">The type of notification event.</param>
        /// <param name="message">The content of the notification message.</param>
        Task LogAsync(int userId, NotificationType type, string message);

        /// <summary>
        /// Retrieves all notification logs for a given user, sorted most recent first.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs to retrieve.</param>
        /// <returns>A list of <see cref="NotificationLogDocument"/> entries.</returns>
        Task<List<NotificationLogDocument>> GetByUserAsync(int userId);
    }
}