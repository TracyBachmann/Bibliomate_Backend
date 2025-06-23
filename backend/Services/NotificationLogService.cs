using backend.Models.Enums;
using backend.Models.Mongo;
using MongoDB.Driver;

namespace backend.Services
{
    /// <summary>
    /// Service for logging notification events to a MongoDB collection.
    /// </summary>
    public class NotificationLogService
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        public NotificationLogService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDb"]);
            var database = client.GetDatabase(config["MongoDbDatabase"]);
            _collection = database.GetCollection<NotificationLogDocument>("NotificationLogs");
        }

        /// <summary>
        /// Logs a notification entry for a user.
        /// </summary>
        public async Task LogAsync(int userId, NotificationType type, string message)
        {
            var log = new NotificationLogDocument
            {
                UserId = userId,
                Type = type,
                Message = message,
                SentAt = DateTime.UtcNow
            };
            await _collection.InsertOneAsync(log);
        }

        /// <summary>
        /// Retrieves all notification logs for a specific user.
        /// </summary>
        public async Task<List<NotificationLogDocument>> GetByUserAsync(int userId)
        {
            return await _collection
                .Find(l => l.UserId == userId)
                .SortByDescending(l => l.SentAt)
                .ToListAsync();
        }
    }
}