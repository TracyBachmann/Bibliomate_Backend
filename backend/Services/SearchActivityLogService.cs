using backend.Configuration;
using backend.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Services
{
    /// <summary>
    /// Service for recording and retrieving search activity logs in MongoDB.
    /// </summary>
    public class SearchActivityLogService
    {
        private readonly IMongoCollection<SearchActivityLogDocument> _collection;

        public SearchActivityLogService(IOptions<MongoSettings> opts, IMongoClient client)
        {
            var settings = opts.Value;
            var db       = client.GetDatabase(settings.DatabaseName);
            _collection  = db.GetCollection<SearchActivityLogDocument>("SearchActivityLogs");

            // Create TTL index to expire logs after 90 days
            var indexKeys = Builders<SearchActivityLogDocument>.IndexKeys.Ascending(d => d.Timestamp);
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) };
            _collection.Indexes.CreateOne(new CreateIndexModel<SearchActivityLogDocument>(indexKeys, indexOptions));
        }

        /// <summary>
        /// Inserts a new search activity log.
        /// </summary>
        public Task LogAsync(SearchActivityLogDocument doc) => _collection.InsertOneAsync(doc);

        /// <summary>
        /// Retrieves search logs for a given user (or all if userId is null).
        /// </summary>
        public Task<List<SearchActivityLogDocument>> GetByUserAsync(int? userId)
        {
            var filter = userId.HasValue
                ? Builders<SearchActivityLogDocument>.Filter.Eq(d => d.UserId, userId.Value)
                : Builders<SearchActivityLogDocument>.Filter.Empty;
            return _collection.Find(filter)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync();
        }
    }
}