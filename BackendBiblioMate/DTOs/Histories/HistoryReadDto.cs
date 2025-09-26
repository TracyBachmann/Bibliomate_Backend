namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when querying a user's history of events.
    /// Contains details about each historical action related to a user account.
    /// </summary>
    public class HistoryReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the history record.
        /// </summary>
        /// <example>123</example>
        public int HistoryId { get; init; }

        /// <summary>
        /// Gets or sets the type of event that occurred.
        /// </summary>
        /// <remarks>
        /// Common values include:
        /// - <c>Loan</c> (book borrowed)  
        /// - <c>Return</c> (book returned)  
        /// - <c>Reservation</c> (reservation placed)  
        /// - <c>Cancel</c> (reservation canceled)  
        /// </remarks>
        /// <example>Loan</example>
        public string EventType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the event occurred, in UTC.
        /// </summary>
        /// <example>2025-06-23T14:30:00Z</example>
        public DateTime EventDate { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the related loan, if applicable.
        /// Returns <c>null</c> if the event is not linked to a loan.
        /// </summary>
        /// <example>45</example>
        public int? LoanId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the related reservation, if applicable.
        /// Returns <c>null</c> if the event is not linked to a reservation.
        /// </summary>
        /// <example>78</example>
        public int? ReservationId { get; init; }
    }
}