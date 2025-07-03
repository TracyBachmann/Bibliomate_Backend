using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations to record and retrieve search activity logs.
    /// </summary>
    public interface ISearchActivityLogService
    {
        /// <summary>
        /// Inserts a new search activity log document into the data store.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="SearchActivityLogDocument"/> containing UserId, QueryText and Timestamp.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the log entry has been persisted.
        /// </returns>
        Task LogAsync(
            SearchActivityLogDocument doc,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all search log documents for a specific user, ordered from newest to oldest.
        /// </summary>
        /// <param name="userId">The identifier of the user whose search logs are requested.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a
        /// <see cref="List{SearchActivityLogDocument}"/> containing the user's search history.
        /// </returns>
        Task<List<SearchActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}