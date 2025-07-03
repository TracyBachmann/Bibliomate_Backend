namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when querying a user's history of events.
    /// Contains details about each historical action related to a user.
    /// </summary>
    public class HistoryReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the history record.
        /// </summary>
        /// <example>123</example>
        public int HistoryId { get; init; }

        /// <summary>
        /// Gets the type of event that occurred.
        /// </summary>
        /// <remarks>
        /// Examples: “Loan”, “Return”, “Reservation”, “Cancel”.
        /// </remarks>
        /// <example>Loan</example>
        public string EventType { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the event occurred (UTC).
        /// </summary>
        /// <example>2025-06-23T14:30:00Z</example>
        public DateTime EventDate { get; init; }

        /// <summary>
        /// Gets the identifier of the related loan, if applicable; otherwise null.
        /// </summary>
        /// <example>45</example>
        public int? LoanId { get; init; }

        /// <summary>
        /// Gets the identifier of the related reservation, if applicable; otherwise null.
        /// </summary>
        /// <example>78</example>
        public int? ReservationId { get; init; }
    }
}