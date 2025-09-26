using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD operations for managing <see cref="NotificationLogDocument"/> entries
    /// stored in MongoDB.  
    /// Provides methods for inserting new logs, retrieving all logs,
    /// and fetching a specific log by its identifier.
    /// </summary>
    public interface IMongoLogService
    {
        /// <summary>
        /// Inserts a new notification log document into the collection.
        /// </summary>
        /// <param name="log">
        /// The <see cref="NotificationLogDocument"/> instance containing
        /// the notification details to be persisted.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the log has been successfully inserted.
        /// </returns>
        Task AddAsync(
            NotificationLogDocument log,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all notification log documents, ordered by their
        /// <see cref="NotificationLogDocument.SentAt"/> property in descending order.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding a <see cref="List{NotificationLogDocument}"/>
        /// containing all log entries, with the most recent first.
        /// </returns>
        Task<List<NotificationLogDocument>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific notification log document by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// The unique string identifier of the notification log document
        /// (typically a MongoDB ObjectId).
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding the matching
        /// <see cref="NotificationLogDocument"/> if found;
        /// otherwise <c>null</c>.
        /// </returns>
        Task<NotificationLogDocument?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default);
    }
}
