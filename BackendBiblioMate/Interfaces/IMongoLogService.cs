using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Contract for CRUD operations on notification log documents.
    /// </summary>
    public interface IMongoLogService
    {
        /// <summary>
        /// Inserts a new notification log document.
        /// </summary>
        /// <param name="log">The <see cref="NotificationLogDocument"/> to insert.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task AddAsync(
            NotificationLogDocument log,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all notification log documents, sorted by sent date descending.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A list of <see cref="NotificationLogDocument"/>, newest first.
        /// </returns>
        Task<List<NotificationLogDocument>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a notification log document by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the log document to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The matching <see cref="NotificationLogDocument"/>, or <c>null</c> if not found.
        /// </returns>
        Task<NotificationLogDocument?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default);
    }
}