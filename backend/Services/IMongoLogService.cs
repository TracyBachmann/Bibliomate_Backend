using backend.Models.Mongo;

public interface IMongoLogService
{
    Task<List<NotificationLogDocument>> GetAllAsync();
    Task<NotificationLogDocument?> GetByIdAsync(string id);
    Task AddAsync(NotificationLogDocument log);
}