using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BackendBiblioMate.Configuration;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Reports
{
    /// <summary>
    /// Provides persistence and retrieval of search activity logs in MongoDB.
    /// Each log entry captures details of a user’s search query for auditing and analytics.
    /// </summary>
    public class SearchActivityLogService : ISearchActivityLogService
    {
        private readonly IMongoCollection<SearchActivityLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchActivityLogService"/> class.
        /// Creates a TTL (time-to-live) index so logs expire automatically after 90 days.
        /// </summary>
        /// <param name="opts">Strongly typed MongoDB connection settings.</param>
        /// <param name="client">MongoDB client instance used to access the database.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="opts"/> or <paramref name="client"/> is <c>null</c>.
        /// </exception>
        public SearchActivityLogService(
            IOptions<MongoSettings> opts,
            IMongoClient client)
        {
            if (opts == null) throw new ArgumentNullException(nameof(opts));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var settings = opts.Value ?? throw new ArgumentNullException(nameof(opts));

            var db = client.GetDatabase(settings.DatabaseName);
            _collection = db.GetCollection<SearchActivityLogDocument>("SearchActivityLogs");

            // Ensure TTL index exists on the Timestamp field
            var keys = Builders<SearchActivityLogDocument>.IndexKeys.Ascending(d => d.Timestamp);
            var options = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) };

            _collection.Indexes.CreateOne(
                new CreateIndexModel<SearchActivityLogDocument>(keys, options));
        }

        /// <summary>
        /// Persists a new search activity log document.
        /// </summary>
        /// <param name="doc">The document containing search details (user ID, query, timestamp).</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous insert operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="doc"/> is <c>null</c>.</exception>
        public Task LogAsync(
            SearchActivityLogDocument doc,
            CancellationToken cancellationToken = default)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            return _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        /// <summary>
        /// Retrieves all search activity logs for a given user,
        /// ordered from most recent to oldest.
        /// </summary>
        /// <param name="userId">The identifier of the user whose search logs are retrieved.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>
        /// A list of <see cref="SearchActivityLogDocument"/> representing the user’s search history.
        /// </returns>
        public Task<List<SearchActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<SearchActivityLogDocument>.Filter.Eq(d => d.UserId, userId);

            return _collection
                .Find(filter)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}
