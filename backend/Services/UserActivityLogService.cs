using backend.Configuration;
using backend.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Services
{
    /// <summary>
    /// Service for recording and retrieving user activity logs from MongoDB.
    /// Implements <see cref="IUserActivityLogService"/>.
    /// </summary>
    public class UserActivityLogService : IUserActivityLogService
    {
        private readonly IMongoCollection<UserActivityLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of <see cref="UserActivityLogService"/>.
        /// </summary>
        /// <param name="opts">
        /// Provides MongoDB settings (ConnectionString & DatabaseName) via <see cref="IOptions{MongoSettings}"/>.
        /// </param>
        /// <param name="client">
        /// Injected <see cref="IMongoClient"/> for connecting to MongoDB.
        /// </param>
        public UserActivityLogService(IOptions<MongoSettings> opts, IMongoClient client)
        {
            var settings = opts.Value;
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<UserActivityLogDocument>("UserActivityLogs");
        }

        /// <inheritdoc/>
        public Task LogAsync(UserActivityLogDocument doc)
        {
            return _collection.InsertOneAsync(doc);
        }

        /// <inheritdoc/>
        public Task<List<UserActivityLogDocument>> GetByUserAsync(int userId)
        {
            return _collection
                .Find(d => d.UserId == userId)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync();
        }
    }
}