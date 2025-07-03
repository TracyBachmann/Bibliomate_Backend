using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations to record and retrieve user activity logs.
    /// </summary>
    public interface IUserActivityLogService
    {
        /// <summary>
        /// Inserts a new user activity log document into the data store.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="UserActivityLogDocument"/> containing UserId, Action, Details and Timestamp.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the document has been inserted.
        /// </returns>
        Task LogAsync(
            UserActivityLogDocument doc,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all activity log documents for a specific user, ordered from newest to oldest.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose logs are requested.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> producing a <see cref="List{UserActivityLogDocument}"/>
        /// sorted by descending <c>Timestamp</c>.
        /// </returns>
        Task<List<UserActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}