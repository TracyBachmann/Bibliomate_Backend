using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Infrastructure.Logging
{
    /// <summary>
    /// Service for raw CRUD operations on notification logs in MongoDB.
    /// </summary>
    public class MongoLogService : IMongoLogService
    {
        private readonly INotificationLogCollection _collection;
        private readonly ILogger<MongoLogService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="MongoLogService"/>.
        /// </summary>
        public MongoLogService(INotificationLogCollection collection, ILogger<MongoLogService> logger)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inserts a new notification log entry.
        /// </summary>
        public async Task AddAsync(NotificationLogDocument log, CancellationToken cancellationToken = default)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            _logger.LogDebug("Inserting log entry with Id={LogId}", log.Id);
            await _collection.InsertOneAsync(log, cancellationToken);
        }

        /// <summary>
        /// Retrieves all notification log entries, sorted by sent date descending.
        /// </summary>
        public async Task<List<NotificationLogDocument>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching all log entries");
            return await _collection.GetAllSortedAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a notification log entry by its unique identifier.
        /// </summary>
        public async Task<NotificationLogDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Log Id must be provided.", nameof(id));

            _logger.LogDebug("Fetching log entry with Id={LogId}", id);
            return await _collection.GetByIdAsync(id, cancellationToken);
        }
    }
}