using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BackendBiblioMate.Configuration;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Users
{
    /// <summary>
    /// Default implementation of <see cref="IUserActivityLogService"/> that records
    /// and retrieves user activity logs stored in MongoDB.
    /// </summary>
    public class UserActivityLogService : IUserActivityLogService
    {
        private readonly IMongoCollection<UserActivityLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserActivityLogService"/> class.
        /// </summary>
        /// <param name="opts">Strongly typed MongoDB settings injected via <see cref="IOptions{TOptions}"/>.</param>
        /// <param name="client">MongoDB client instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="opts"/> or <paramref name="client"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the configured <c>DatabaseName</c> is missing or empty.
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

        /// <inheritdoc />
        /// <summary>
        /// Persists a new user activity log entry into the MongoDB collection.
        /// </summary>
        /// <param name="doc">The <see cref="UserActivityLogDocument"/> to insert.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="doc"/> is <c>null</c>.</exception>
        public Task LogAsync(
            UserActivityLogDocument doc,
            CancellationToken cancellationToken = default)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            return _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves all user activity logs for a given user,
        /// sorted by <see cref="UserActivityLogDocument.Timestamp"/> in descending order.
        /// </summary>
        /// <param name="userId">The identifier of the user whose activity logs are retrieved.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A list of <see cref="UserActivityLogDocument"/> entries for the user.</returns>
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
