using backend.Models.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using backend.Configuration;

namespace backend.Services
{
    public class MongoLogService
    {
        private readonly IMongoCollection<NotificationLogDocument> _logCollection;

        public MongoLogService(IOptions<MongoSettings> mongoSettings, IMongoClient mongoClient)
        {
            var db = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _logCollection = db.GetCollection<NotificationLogDocument>("logEntries");
        }

        public async Task<List<NotificationLogDocument>> GetAllAsync()
            => await _logCollection.Find(_ => true).ToListAsync();

        public async Task<NotificationLogDocument?> GetByIdAsync(string id)
            => await _logCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task AddAsync(NotificationLogDocument log)
            => await _logCollection.InsertOneAsync(log);
    }
}