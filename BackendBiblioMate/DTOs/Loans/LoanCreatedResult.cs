namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned by the loan creation workflow.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This DTO is produced when a loan is successfully created (e.g., by <c>ILoanService.CreateAsync</c>).
    /// It is a lightweight response containing only the information the client needs immediately after
    /// creation—specifically, the computed due date.
    /// </para>
    /// <para>
    /// This type is not an entity and is not persisted by itself. It is designed for API responses
    /// and client consumption.
    /// </para>
    /// </remarks>
    public class LoanCreatedResult
    {
        /// <summary>
        /// Gets the date and time when the borrowed item is due to be returned.
        /// </summary>
        /// <remarks>
        /// The value is expressed in Coordinated Universal Time (UTC). Clients should convert
        /// to the appropriate local time zone for display purposes.
        /// </remarks>
        /// <example>2025-06-15T10:30:00Z</example>
        public DateTime DueDate { get; init; }
    }
}