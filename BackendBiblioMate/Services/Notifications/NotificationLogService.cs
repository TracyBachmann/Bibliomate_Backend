using MongoDB.Driver;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Notifications
{
    /// <summary>
    /// Default implementation of <see cref="INotificationLogService"/>.
    /// Responsible for persisting notification log entries in MongoDB
    /// and retrieving them for a specific user.
    /// </summary>
    public class NotificationLogService : INotificationLogService
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationLogService"/> class.
        /// Establishes a MongoDB connection and retrieves the notification log collection.
        /// </summary>
        /// <param name="config">The application configuration containing MongoDB connection settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if required MongoDB settings (<c>MongoDb:ConnectionString</c> or <c>MongoDb:DatabaseName</c>) are missing.
        /// </exception>
        public NotificationLogService(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var connectionString = config["MongoDb:ConnectionString"];
            var databaseName     = config["MongoDb:DatabaseName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException("MongoDB settings missing.");

            var client   = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _collection = database.GetCollection<NotificationLogDocument>("NotificationLogs");
        }

        /// <summary>
        /// Creates and inserts a new notification log entry into MongoDB.
        /// </summary>
        /// <param name="userId">The identifier of the user who received the notification.</param>
        /// <param name="type">The type of the notification (e.g., reminder, overdue, system alert).</param>
        /// <param name="message">The message content of the notification.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous insert operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="message"/> is null or whitespace.</exception>
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
        /// Retrieves all notification logs for a specific user, ordered by most recent first.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs should be retrieved.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the asynchronous operation.</param>
        /// <returns>
        /// A list of <see cref="NotificationLogDocument"/> objects corresponding to the user,
        /// sorted in descending order by <see cref="NotificationLogDocument.SentAt"/>.
        /// </returns>
        public async Task<List<NotificationLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<NotificationLogDocument>.Filter.Eq(d => d.UserId, userId);

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
