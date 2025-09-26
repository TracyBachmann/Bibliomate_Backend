namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines a service responsible for cleaning up expired reservations.
    /// Expired reservations are those that remain unclaimed after the allowed
    /// availability window (e.g., 48 hours after notification).
    /// </summary>
    public interface IReservationCleanupService
    {
        /// <summary>
        /// Removes all reservations that have exceeded their expiration window,
        /// restores the associated stock to availability, logs each expiration
        /// event for audit purposes, and returns the number of reservations removed.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields the number of expired
        /// reservations successfully removed from the system.
        /// </returns>
        Task<int> CleanupExpiredReservationsAsync(
            CancellationToken cancellationToken = default);
    }
}