using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Abstraction over the underlying MongoDB collection for notification logs.
    /// </summary>
    public interface INotificationLogCollection
    {
        /// <summary>
        /// Inserts a new notification log document.
        /// </summary>
        Task InsertOneAsync(NotificationLogDocument doc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all notification logs, sorted descending by SentAt.
        /// </summary>
        Task<List<NotificationLogDocument>> GetAllSortedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a notification log document by its ID.
        /// </summary>
        Task<NotificationLogDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}