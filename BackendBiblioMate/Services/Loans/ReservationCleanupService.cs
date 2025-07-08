using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Removes expired reservations, restores stock availability, logs each expiration event,
    /// and returns the total count of removed reservations.
    /// </summary>
    public class ReservationCleanupService : IReservationCleanupService
    {
        private readonly BiblioMateDbContext _context;
        private readonly IHistoryService _historyService;

        /// <summary>
        /// Number of hours after which an available reservation expires.
        /// </summary>
        private const int ExpirationWindowHours = 48;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationCleanupService"/> class.
        /// </summary>
        /// <param name="context">The database context used to access reservations and stock data.</param>
        /// <param name="historyService">The service used to log history events.</param>
        public ReservationCleanupService(
            BiblioMateDbContext context,
            IHistoryService historyService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        /// <summary>
        /// Finds and removes all reservations in the "Available" status that have
        /// been available for longer than the expiration window, restores any assigned stock,
        /// logs the expiration to history, and saves the changes.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>The number of reservations that were removed.</returns>
        public async Task<int> CleanupExpiredReservationsAsync(
            CancellationToken cancellationToken = default)
        {
            var expirationThreshold = DateTime.UtcNow.AddHours(-ExpirationWindowHours);

            var expiredReservations = await _context.Reservations
                .Where(r =>
                    r.Status == ReservationStatus.Available &&
                    r.AvailableAt.HasValue &&
                    r.AvailableAt.Value <= expirationThreshold)
                .ToListAsync(cancellationToken);

            foreach (var reservation in expiredReservations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reservation.AssignedStockId.HasValue)
                {
                    await RestoreStockAsync(reservation.AssignedStockId.Value, cancellationToken);
                }

                await _historyService.LogEventAsync(
                    userId: reservation.UserId,
                    eventType: "ReservationExpired",
                    reservationId: reservation.ReservationId,
                    cancellationToken: cancellationToken);

                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return expiredReservations.Count;
        }

        /// <summary>
        /// Marks the specified stock as available again.
        /// </summary>
        /// <param name="stockId">The identifier of the stock record to update.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task RestoreStockAsync(int stockId, CancellationToken cancellationToken)
        {
            return _context.Stocks
                .Where(s => s.StockId == stockId)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.IsAvailable, true), cancellationToken);
        }
    }
}