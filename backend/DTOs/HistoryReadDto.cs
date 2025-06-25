namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when querying a user's history of events.
    /// </summary>
    public class HistoryReadDto
    {
        /// <summary>
        /// Unique identifier of the history record.
        /// </summary>
        /// <example>123</example>
        public int HistoryId { get; set; }

        /// <summary>
        /// Type of event that occurred (e.g., "Loan", "Return", "Reservation", "Cancel").
        /// </summary>
        /// <example>Loan</example>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the event occurred.
        /// </summary>
        /// <example>2025-06-23T14:30:00Z</example>
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Identifier of the related loan, if applicable; otherwise null.
        /// </summary>
        /// <example>45</example>
        public int? LoanId { get; set; }

        /// <summary>
        /// Identifier of the related reservation, if applicable; otherwise null.
        /// </summary>
        /// <example>78</example>
        public int? ReservationId { get; set; }
    }
}