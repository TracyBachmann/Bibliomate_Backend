namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned after processing a loan return.
    /// Includes fine information and reservation notification status.
    /// </summary>
    public class LoanReturnedResult
    {
        /// <summary>
        /// Gets or sets whether a reservation notification was sent for the returned book.
        /// </summary>
        /// <example>true</example>
        public bool ReservationNotified { get; set; }

        /// <summary>
        /// Gets or sets the fine amount charged for late return, if any.
        /// </summary>
        /// <example>5.00</example>
        public decimal Fine { get; set; }
    }
}