using MongoDB.Driver;
using backend.Models.Enums;
using backend.Models.Mongo;

namespace backend.Services
{
    /// <summary>
    /// Service for logging and retrieving notification events in a MongoDB collection.
    /// </summary>
    public class NotificationLogService
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        /// <summary>
        /// Constructs the NotificationLogService with MongoDB configuration.
        /// </summary>
        /// <param name="config">
        /// Configuration providing "MongoDb" connection string and "MongoDbDatabase" name.
        /// </param>
        public NotificationLogService(IConfiguration config)
        {
            var client   = new MongoClient(config["MongoDb"]);
            var database = client.GetDatabase(config["MongoDbDatabase"]);
            _collection  = database.GetCollection<NotificationLogDocument>("NotificationLogs");
        }

        /// <summary>
        /// Inserts a new notification log into MongoDB.
        /// </summary>
        /// <param name="userId">Identifier of the user who received the notification.</param>
        /// <param name="type">Type of notification (e.g., ReservationAvailable, ReturnReminder).</param>
        /// <param name="message">Content of the notification message.</param>
        public async Task LogAsync(int userId, NotificationType type, string message)
        {
            var log = new NotificationLogDocument
            {
                UserId = userId,
                Type   = type,
                Message= message,
                SentAt = DateTime.UtcNow
            };
            await _collection.InsertOneAsync(log);
        }

        /// <summary>
        /// Retrieves all notification logs for a given user, sorted by most recent first.
        /// </summary>
        /// <param name="userId">Identifier of the user whose logs to retrieve.</param>
        /// <returns>
        /// A list of <see cref="NotificationLogDocument"/> entries for that user.
        /// </returns>
        public async Task<List<NotificationLogDocument>> GetByUserAsync(int userId)
        {
            return await _collection
                .Find(l => l.UserId == userId)
                .SortByDescending(l => l.SentAt)
                .ToListAsync();
        }
    }
}
