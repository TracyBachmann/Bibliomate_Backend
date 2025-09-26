using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for recording and retrieving user activity logs.
    /// These logs capture significant user actions (e.g., login, book search, reservation)
    /// for audit trails, analytics, and behavior tracking.
    /// </summary>
    public interface IUserActivityLogService
    {
        /// <summary>
        /// Records a new user activity log entry in the data store.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="UserActivityLogDocument"/> containing:
        /// <list type="bullet">
        ///   <item><c>UserId</c> — the identifier of the user.</item>
        ///   <item><c>Action</c> — the type of activity performed (e.g. "Login", "Search").</item>
        ///   <item><c>Details</c> — optional descriptive metadata about the action.</item>
        ///   <item><c>Timestamp</c> — when the action occurred.</item>
        /// </list>
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the activity has been persisted.
        /// </returns>
        Task LogAsync(
            UserActivityLogDocument doc,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all activity logs for a specific user, ordered from most recent to oldest.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose logs are requested.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a list of <see cref="UserActivityLogDocument"/>,
        /// sorted by descending <c>Timestamp</c>, representing the user’s activity history.
        /// </returns>
        Task<List<UserActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
