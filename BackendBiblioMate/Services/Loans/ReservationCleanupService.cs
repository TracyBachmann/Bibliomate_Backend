using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Service that periodically cleans up expired reservations:
    /// - Removes reservations in <see cref="ReservationStatus.Available"/> state
    ///   that exceeded the pickup window.
    /// - Restores the associated stock availability.
    /// - Logs each expiration event to the history system.
    /// </summary>
    public class ReservationCleanupService : IReservationCleanupService
    {
        private readonly BiblioMateDbContext _context;
        private readonly IHistoryService _historyService;

        /// <summary>
        /// Number of hours after which a reservation becomes expired
        /// if the reserved copy has not been collected.
        /// </summary>
        private const int ExpirationWindowHours = 48;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationCleanupService"/> class.
        /// </summary>
        /// <param name="context">EF Core database context for reservations and stock.</param>
        /// <param name="historyService">Service for logging reservation expiration events.</param>
        /// <exception cref="ArgumentNullException">Thrown if a dependency is null.</exception>
        public ReservationCleanupService(
            BiblioMateDbContext context,
            IHistoryService historyService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        /// <summary>
        /// Scans for expired reservations, restores related stock availability,
        /// logs each expiration event, and removes the reservation records.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Total number of expired reservations removed.</returns>
        public async Task<int> CleanupExpiredReservationsAsync(
            CancellationToken cancellationToken = default)
        {
            var expirationThreshold = DateTime.UtcNow.AddHours(-ExpirationWindowHours);

            // Find reservations that are still "Available" but have passed their pickup window
            var expiredReservations = await _context.Reservations
                .Where(r =>
                    r.Status == ReservationStatus.Available &&
                    r.AvailableAt.HasValue &&
                    r.AvailableAt.Value <= expirationThreshold)
                .ToListAsync(cancellationToken);

            foreach (var reservation in expiredReservations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Restore stock availability if an item was pre-assigned
                if (reservation.AssignedStockId.HasValue)
                {
                    await RestoreStockAsync(reservation.AssignedStockId.Value, cancellationToken);
                }

                // Log reservation expiration into the history
                await _historyService.LogEventAsync(
                    userId: reservation.UserId,
                    eventType: "ReservationExpired",
                    reservationId: reservation.ReservationId,
                    cancellationToken: cancellationToken);

                _context.Reservations.Remove(reservation);
            }

            // Commit changes (both deletions and stock updates)
            await _context.SaveChangesAsync(cancellationToken);

            return expiredReservations.Count;
        }

        /// <summary>
        /// Marks the specified stock record as available again.
        /// </summary>
        /// <param name="stockId">Identifier of the stock to restore.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        private Task RestoreStockAsync(int stockId, CancellationToken cancellationToken)
        {
            return _context.Stocks
                .Where(s => s.StockId == stockId)
                .ExecuteUpdateAsync(
                    updater => updater.SetProperty(s => s.IsAvailable, true),
                    cancellationToken);
        }
    }
}
