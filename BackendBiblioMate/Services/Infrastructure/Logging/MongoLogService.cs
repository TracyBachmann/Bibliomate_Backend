using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Infrastructure.Logging
{
    /// <summary>
    /// Provides low-level CRUD operations for notification logs stored in MongoDB.
    /// </summary>
    public class MongoLogService : IMongoLogService
    {
        private readonly INotificationLogCollection _collection;
        private readonly ILogger<MongoLogService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="MongoLogService"/>.
        /// </summary>
        /// <param name="collection">Abstraction over the MongoDB notification log collection.</param>
        /// <param name="logger">Logger instance for diagnostics.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="collection"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public MongoLogService(
            INotificationLogCollection collection,
            ILogger<MongoLogService> logger)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _logger     = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inserts a new notification log entry into the MongoDB collection.
        /// </summary>
        /// <param name="log">The notification log document to insert.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="log"/> is <c>null</c>.
        /// </exception>
        public async Task AddAsync(
            NotificationLogDocument log,
            CancellationToken cancellationToken = default)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            _logger.LogDebug("Inserting notification log entry with Id={LogId}", log.Id);
            await _collection.InsertOneAsync(log, cancellationToken);
        }

        /// <summary>
        /// Retrieves all notification logs, sorted in descending order by their sent date.
        /// </summary>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <returns>
        /// A list of <see cref="NotificationLogDocument"/> objects representing the log history.
        /// </returns>
        public async Task<List<NotificationLogDocument>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching all notification log entries");
            return await _collection.GetAllSortedAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a specific notification log entry by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the log document.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <returns>
        /// The matching <see cref="NotificationLogDocument"/> if found; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="id"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        public async Task<NotificationLogDocument?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Log Id must be provided.", nameof(id));

            _logger.LogDebug("Fetching notification log entry with Id={LogId}", id);
            return await _collection.GetByIdAsync(id, cancellationToken);
        }
    }
}
