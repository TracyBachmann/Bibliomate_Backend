using MongoDB.Driver;
using backend.Models.Enums;
using backend.Models.Mongo;

namespace backend.Services
{
    /// <summary>
    /// Service for logging and retrieving notification events in MongoDB.
    /// </summary>
    public class NotificationLogService : INotificationLogService
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        /// <summary>
        /// Constructs a new <see cref="NotificationLogService"/>.
        /// </summary>
        /// <param name="config">
        /// Configuration providing "MongoDb" connection string and "MongoDbDatabase" name.
        /// </param>
        public NotificationLogService(IConfiguration config)
        {
            var connectionString = config["MongoDb:ConnectionString"];
            var databaseName     = config["MongoDb:DatabaseName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("MongoDB connection settings are missing or incomplete.");
            }

            var client   = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection  = database.GetCollection<NotificationLogDocument>("NotificationLogs");
        }

        /// <inheritdoc/>
        public async Task LogAsync(int userId, NotificationType type, string message)
        {
            var log = new NotificationLogDocument
            {
                UserId  = userId,
                Type    = type,
                Message = message,
                SentAt  = DateTime.UtcNow
            };
            await _collection.InsertOneAsync(log);
        }

        /// <inheritdoc/>
        public async Task<List<NotificationLogDocument>> GetByUserAsync(int userId)
        {
            return await _collection
                .Find(l => l.UserId == userId)
                .SortByDescending(l => l.SentAt)
                .ToListAsync();
        }
    }
}
