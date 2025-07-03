using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BackendBiblioMate.Configuration;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Reports
{
    /// <summary>
    /// Service for recording and retrieving search activity logs in MongoDB.
    /// </summary>
    public class SearchActivityLogService : ISearchActivityLogService
    {
        private readonly IMongoCollection<SearchActivityLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of <see cref="SearchActivityLogService"/>.
        /// </summary>
        /// <param name="opts">MongoDB connection settings.</param>
        /// <param name="client">MongoDB client instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="opts"/> or <paramref name="client"/> is null.
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

            // Create TTL index to expire logs after 90 days
            var keys = Builders<SearchActivityLogDocument>.IndexKeys.Ascending(d => d.Timestamp);
            var optionsIndex = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) };
            _collection.Indexes.CreateOne(new CreateIndexModel<SearchActivityLogDocument>(keys, optionsIndex));
        }

        /// <summary>
        /// Inserts a new search activity log document.
        /// </summary>
        /// <param name="doc">Document containing search activity details.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>Asynchronous task representing the insert operation.</returns>
        public Task LogAsync(
            SearchActivityLogDocument doc,
            CancellationToken cancellationToken = default)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            return _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        /// <summary>
        /// Retrieves search activity logs for a user, ordered by timestamp descending.
        /// </summary>
        /// <param name="userId">Identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>
        /// List of <see cref="SearchActivityLogDocument"/> for the user.
        /// </returns>
        public Task<List<SearchActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<SearchActivityLogDocument>
                .Filter.Eq(d => d.UserId, userId);

            return _collection
                .Find(filter)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}