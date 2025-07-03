namespace BackendBiblioMate.Models.Enums
{
    /// <summary>
    /// Defines the possible states of a reservation in the library system.
    /// </summary>
    public enum ReservationStatus
    {
        /// <summary>
        /// Reservation has been created and is awaiting availability of the item.
        /// </summary>
        Pending,

        /// <summary>
        /// The reserved item is available for the user to collect.
        /// </summary>
        Available,

        /// <summary>
        /// The reservation process is completed; the user has collected and returned the item.
        /// </summary>
        Completed,

        /// <summary>
        /// The reservation has been cancelled either by the user or due to expiration.
        /// </summary>
        Cancelled
    }
}