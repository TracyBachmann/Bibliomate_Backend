using backend.Configuration;
using backend.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Services
{
    /// <summary>
    /// Service for recording and retrieving user activity logs in MongoDB.
    /// </summary>
    public class UserActivityLogService
    {
        private readonly IMongoCollection<UserActivityLogDocument> _collection;

        /// <summary>
        /// Initializes the service with MongoDB configuration.
        /// </summary>
        /// <param name="opts">
        /// Provides the "MongoDb" section (ConnectionString & DatabaseName) via IOptions.
        /// </param>
        /// <param name="client">
        /// Singleton MongoDB client injected by DI.
        /// </param>
        public UserActivityLogService(IOptions<MongoSettings> opts, IMongoClient client)
        {
            var settings = opts.Value;
            // Get or create the database specified in configuration
            var database = client.GetDatabase(settings.DatabaseName);
            // Get or create the "UserActivityLogs" collection
            _collection = database.GetCollection<UserActivityLogDocument>("UserActivityLogs");
        }

        /// <summary>
        /// Inserts a new user activity log into the MongoDB collection.
        /// </summary>
        /// <param name="doc">
        /// The activity document containing userId, action, details, and timestamp.
        /// </param>
        public Task LogAsync(UserActivityLogDocument doc) =>
            _collection.InsertOneAsync(doc);

        /// <summary>
        /// Retrieves all activity logs for a specific user, ordered from newest to oldest.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose activity logs are requested.
        /// </param>
        /// <returns>
        /// A list of <see cref="UserActivityLogDocument"/> sorted by descending timestamp.
        /// </returns>
        public Task<List<UserActivityLogDocument>> GetByUserAsync(int userId) =>
            _collection
                .Find(d => d.UserId == userId)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync();
    }
}
