namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines an abstraction for cleaning up expired reservations.
    /// </summary>
    public interface IReservationCleanupService
    {
        /// <summary>
        /// Removes all reservations that have been available for more than the expiration window,
        /// restores stock, logs each expiration, and returns the number removed.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation.</param>
        /// <returns>The number of expired reservations that were removed.</returns>
        Task<int> CleanupExpiredReservationsAsync(CancellationToken cancellationToken = default);
    }
}