using MongoDB.Driver;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Notifications
{
    /// <summary>
    /// Service for logging notification events to MongoDB and retrieving them by user.
    /// </summary>
    public class NotificationLogService : INotificationLogService
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of <see cref="NotificationLogService"/>.
        /// </summary>
        /// <param name="config">Application configuration containing MongoDB settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if MongoDB settings are missing.</exception>
        public NotificationLogService(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var connectionString = config["MongoDb:ConnectionString"];
            var databaseName     = config["MongoDb:DatabaseName"];
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException("MongoDB settings missing.");

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<NotificationLogDocument>("NotificationLogs");
        }

        /// <summary>
        /// Inserts a new notification log entry for the specified user.
        /// </summary>
        /// <param name="userId">Identifier of the user who received the notification.</param>
        /// <param name="type">Type of the notification sent.</param>
        /// <param name="message">Content of the notification message.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the insert operation.</returns>
        public async Task LogAsync(
            int userId,
            NotificationType type,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.", nameof(message));

            var doc = new NotificationLogDocument
            {
                UserId  = userId,
                Type    = type,
                Message = message,
                SentAt  = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        /// <summary>
        /// Retrieves all notification log entries for the specified user, ordered by most recent first.
        /// </summary>
        /// <param name="userId">Identifier of the user whose logs are retrieved.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>
        /// List of <see cref="NotificationLogDocument"/> for the user,
        /// sorted in descending order by <see cref="NotificationLogDocument.SentAt"/>.
        /// </returns>
        public async Task<List<NotificationLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<NotificationLogDocument>
                .Filter.Eq(d => d.UserId, userId);

            using var cursor = await _collection.FindAsync(filter, options: null, cancellationToken);
            var all = new List<NotificationLogDocument>();

            while (await cursor.MoveNextAsync(cancellationToken))
            {
                all.AddRange(cursor.Current);
            }

            return all
                .OrderByDescending(d => d.SentAt)
                .ToList();
        }
    }
}