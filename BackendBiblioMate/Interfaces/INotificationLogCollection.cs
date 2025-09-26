using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Abstraction over the underlying MongoDB collection 
    /// responsible for storing <see cref="NotificationLogDocument"/> entries.
    /// Provides low-level data access methods that are typically wrapped by higher-level services.
    /// </summary>
    public interface INotificationLogCollection
    {
        /// <summary>
        /// Inserts a new notification log document into the collection.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="NotificationLogDocument"/> instance to insert.
        /// This parameter must not be <c>null</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to observe cancellation requests while the operation is executing.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes once the document has been persisted.
        /// </returns>
        Task InsertOneAsync(
            NotificationLogDocument doc,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all notification logs, ordered by 
        /// <see cref="NotificationLogDocument.SentAt"/> in descending order 
        /// (most recent first).
        /// </summary>
        /// <param name="cancellationToken">
        /// A token used to observe cancellation requests while the operation is executing.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a 
        /// <see cref="List{NotificationLogDocument}"/> containing all log entries.
        /// </returns>
        Task<List<NotificationLogDocument>> GetAllSortedAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a notification log document by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the log entry (typically the MongoDB ObjectId as a string).
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to observe cancellation requests while the operation is executing.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding the matching 
        /// <see cref="NotificationLogDocument"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<NotificationLogDocument?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default);
    }
}
