using BackendBiblioMate.Configuration;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BackendBiblioMate.Services.Infrastructure.Logging
{
    /// <summary>
    /// MongoDB-backed implementation of <see cref="INotificationLogCollection"/>.
    /// Provides CRUD-like access to notification log documents for auditing purposes.
    /// </summary>
    public class NotificationLogCollection : INotificationLogCollection
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        /// <summary>
        /// Initializes a new instance of <see cref="NotificationLogCollection"/>.
        /// </summary>
        /// <param name="mongoClient">The MongoDB client instance.</param>
        /// <param name="settings">Application MongoDB configuration (database and collection names).</param>
        /// <exception cref="ArgumentNullException">Thrown if required settings are missing.</exception>
        public NotificationLogCollection(IMongoClient mongoClient, IOptions<MongoSettings> settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Value.DatabaseName))
                throw new ArgumentNullException(nameof(settings.Value.DatabaseName), "MongoDB DatabaseName is missing in configuration.");
            if (string.IsNullOrWhiteSpace(settings.Value.LogCollectionName))
                throw new ArgumentNullException(nameof(settings.Value.LogCollectionName), "MongoDB LogCollectionName is missing in configuration.");

            var db = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collection = db.GetCollection<NotificationLogDocument>(settings.Value.LogCollectionName);
        }

        /// <summary>
        /// Inserts a single <see cref="NotificationLogDocument"/> into the collection.
        /// </summary>
        /// <param name="doc">The document to insert.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public Task InsertOneAsync(NotificationLogDocument doc, CancellationToken cancellationToken = default)
        {
            return _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        /// <summary>
        /// Retrieves all notification log documents, sorted by <see cref="NotificationLogDocument.SentAt"/> in descending order.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of notification logs, most recent first.</returns>
        public async Task<List<NotificationLogDocument>> GetAllSortedAsync(CancellationToken cancellationToken = default)
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(d => d.SentAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single notification log document by its identifier.
        /// </summary>
        /// <param name="id">The MongoDB identifier (string).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The matching document, or <c>null</c> if not found.</returns>
        public async Task<NotificationLogDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<NotificationLogDocument>.Filter.Eq(d => d.Id, id);
            return await _collection
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
