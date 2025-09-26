using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for recording and retrieving user history events,
    /// such as book loans, returns, and reservations.
    /// </summary>
    public interface IHistoryService
    {
        /// <summary>
        /// Records a new history event for a user.
        /// </summary>
        /// <param name="userId">
        /// Identifier of the user who generated the event.
        /// </param>
        /// <param name="eventType">
        /// Type or name of the event (e.g., <c>"Loan"</c>, <c>"Return"</c>, <c>"Reservation"</c>).
        /// </param>
        /// <param name="loanId">
        /// Optional identifier of the related loan, if the event is loan-related.
        /// </param>
        /// <param name="reservationId">
        /// Optional identifier of the related reservation, if the event is reservation-related.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation
        /// of persisting the history event.
        /// </returns>
        Task LogEventAsync(
            int userId,
            string eventType,
            int? loanId = null,
            int? reservationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a paginated list of history events for a specific user.
        /// </summary>
        /// <param name="userId">
        /// Identifier of the user whose history is requested.
        /// </param>
        /// <param name="page">
        /// Page number (1-based). Defaults to <c>1</c>.
        /// </param>
        /// <param name="pageSize">
        /// Number of events per page. Defaults to <c>20</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed,
        /// yields a <see cref="List{HistoryReadDto}"/> containing
        /// the requested page of history events in chronological order.
        /// </returns>
        Task<List<HistoryReadDto>> GetHistoryForUserAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}
