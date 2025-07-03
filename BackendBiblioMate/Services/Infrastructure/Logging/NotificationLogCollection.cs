using BackendBiblioMate.Configuration;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BackendBiblioMate.Services.Infrastructure.Logging
{
    /// <summary>
    /// Default implementation of <see cref="INotificationLogCollection"/> that wraps the real MongoDB collection.
    /// </summary>
    public class NotificationLogCollection : INotificationLogCollection
    {
        private readonly IMongoCollection<NotificationLogDocument> _collection;

        public NotificationLogCollection(IMongoClient mongoClient, IOptions<MongoSettings> settings)
        {
            var db = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collection = db.GetCollection<NotificationLogDocument>(settings.Value.LogCollectionName);
        }

        public Task InsertOneAsync(NotificationLogDocument doc, CancellationToken cancellationToken = default)
        {
            return _collection.InsertOneAsync(doc, options: null, cancellationToken);
        }

        public async Task<List<NotificationLogDocument>> GetAllSortedAsync(CancellationToken cancellationToken = default)
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(d => d.SentAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<NotificationLogDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<NotificationLogDocument>.Filter.Eq(d => d.Id, id);
            return await _collection
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}