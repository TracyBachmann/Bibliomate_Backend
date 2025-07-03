using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations to log and retrieve user history events.
    /// </summary>
    public interface IHistoryService
    {
        /// <summary>
        /// Records a new history event for a user.
        /// </summary>
        /// <param name="userId">Identifier of the user who generated the event.</param>
        /// <param name="eventType">
        /// Type or name of the event (e.g., "Loan", "Return").
        /// </param>
        /// <param name="loanId">
        /// Optional associated loan identifier.
        /// </param>
        /// <param name="reservationId">
        /// Optional associated reservation identifier.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the event has been logged.
        /// </returns>
        Task LogEventAsync(
            int userId,
            string eventType,
            int? loanId = null,
            int? reservationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a page of history events for a given user.
        /// </summary>
        /// <param name="userId">Identifier of the user whose history is requested.</param>
        /// <param name="page">Page number (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Number of items per page. Default is <c>20</c>.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a 
        /// <see cref="List{HistoryReadDto}"/> containing the requested page of history events.
        /// </returns>
        Task<List<HistoryReadDto>> GetHistoryForUserAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}