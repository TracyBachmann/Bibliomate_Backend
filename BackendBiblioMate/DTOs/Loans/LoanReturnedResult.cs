namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when a loan is returned, including fine and notification flag.
    /// </summary>
    public class LoanReturnedResult
    {
        /// <summary>
        /// Gets or sets whether a reservation notification was sent.
        /// </summary>
        public bool ReservationNotified { get; set; }

        /// <summary>
        /// Gets or sets the fine amount charged for late return, if any.
        /// </summary>
        public decimal Fine { get; set; }
    }
}