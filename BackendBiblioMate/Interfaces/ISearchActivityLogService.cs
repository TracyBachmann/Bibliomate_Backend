using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for recording and retrieving user search activity logs.
    /// Used for analytics, personalization, and auditing purposes.
    /// </summary>
    public interface ISearchActivityLogService
    {
        /// <summary>
        /// Persists a new search activity log entry into the underlying data store.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="SearchActivityLogDocument"/> containing:
        /// <list type="bullet">
        ///   <item><description><c>UserId</c>: The identifier of the user performing the search.</description></item>
        ///   <item><description><c>QueryText</c>: The search string entered by the user.</description></item>
        ///   <item><description><c>Timestamp</c>: The UTC time at which the search was performed.</description></item>
        /// </list>
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes once the log entry has been successfully stored.
        /// </returns>
        Task LogAsync(
            SearchActivityLogDocument doc,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all search activity logs for a specific user, ordered from most recent to oldest.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose search history is to be retrieved.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a 
        /// <see cref="List{SearchActivityLogDocument}"/> containing the user's search history.
        /// If the user has no logged searches, an empty list is returned.
        /// </returns>
        Task<List<SearchActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
