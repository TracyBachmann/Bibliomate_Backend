using backend.Configuration;
using backend.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Services
{
    /// <summary>
    /// Service for recording and retrieving search activity logs in MongoDB.
    /// Implements <see cref="ISearchActivityLogService"/>.
    /// </summary>
    public class SearchActivityLogService : ISearchActivityLogService
    {
        private readonly IMongoCollection<SearchActivityLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of <see cref="SearchActivityLogService"/>.
        /// </summary>
        /// <param name="opts">MongoDB connection settings.</param>
        /// <param name="client">MongoDB client instance.</param>
        public SearchActivityLogService(IOptions<MongoSettings> opts, IMongoClient client)
        {
            var settings = opts.Value;
            var db       = client.GetDatabase(settings.DatabaseName);
            _collection  = db.GetCollection<SearchActivityLogDocument>("SearchActivityLogs");

            // Create TTL index to expire logs after 90 days
            var indexKeys    = Builders<SearchActivityLogDocument>.IndexKeys.Ascending(d => d.Timestamp);
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) };
            _collection.Indexes.CreateOne(new CreateIndexModel<SearchActivityLogDocument>(indexKeys, indexOptions));
        }

        /// <summary>
        /// Inserts a new search activity log document into the MongoDB collection.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="SearchActivityLogDocument"/> containing UserId, QueryText and Timestamp.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the document has been inserted.
        /// </returns>
        public Task LogAsync(SearchActivityLogDocument doc)
            => _collection.InsertOneAsync(doc);

        /// <summary>
        /// Retrieves all search activity logs for a specific user,
        /// ordered from newest to oldest.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs are requested.</param>
        /// <returns>
        /// A <see cref="Task{List}"/> producing a <see cref="List{SearchActivityLogDocument}"/>
        /// sorted by descending <c>Timestamp</c>.
        /// </returns>
        public Task<List<SearchActivityLogDocument>> GetByUserAsync(int userId)
        {
            var filter = Builders<SearchActivityLogDocument>.Filter.Eq(d => d.UserId, userId);
            return _collection.Find(filter)
                              .SortByDescending(d => d.Timestamp)
                              .ToListAsync();
        }
    }
}
