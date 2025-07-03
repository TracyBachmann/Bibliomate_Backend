using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BackendBiblioMate.Configuration;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Users
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
        /// MongoDB connection settings (ConnectionString &amp; DatabaseName).
        /// </param>
        /// <param name="client">MongoDB client instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="opts"/> or <paramref name="client"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the configured database name is null or empty.
        /// </exception>
        public UserActivityLogService(
            IOptions<MongoSettings> opts,
            IMongoClient client)
        {
            if (opts == null) throw new ArgumentNullException(nameof(opts));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var settings = opts.Value ?? throw new ArgumentNullException(nameof(opts), "MongoSettings value is null.");
            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new InvalidOperationException("MongoSettings.DatabaseName is not configured.");

            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<UserActivityLogDocument>("UserActivityLogs");
        }

        /// <summary>
        /// Inserts a new user activity log document into MongoDB.
        /// </summary>
        /// <param name="doc">The log document to insert.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the insert operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="doc"/> is null.</exception>
        public Task LogAsync(
            UserActivityLogDocument doc,
            CancellationToken cancellationToken = default)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            return _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        /// <summary>
        /// Retrieves all activity log documents for the specified user,
        /// ordered by most recent first.
        /// </summary>
        /// <param name="userId">Identifier of the user whose logs are retrieved.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>
        /// List of <see cref="UserActivityLogDocument"/> for the user,
        /// sorted in descending order by <see cref="UserActivityLogDocument.Timestamp"/>.
        /// </returns>
        public Task<List<UserActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserActivityLogDocument>.Filter.Eq(d => d.UserId, userId);
            return _collection
                .Find(filter)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}