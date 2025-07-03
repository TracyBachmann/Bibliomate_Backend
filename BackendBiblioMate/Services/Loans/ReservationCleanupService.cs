using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Users;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Service that purges expired reservations, restores stock, logs each expiration,
    /// and returns the count of removed reservations.
    /// </summary>
    public class ReservationCleanupService
    {
        private readonly BiblioMateDbContext _context;
        private readonly HistoryService _historyService;

        /// <summary>
        /// Time window (in hours) after which an available reservation expires.
        /// </summary>
        private const int ExpirationWindowHours = 48;

        /// <summary>
        /// Initializes a new instance of <see cref="ReservationCleanupService"/>.
        /// </summary>
        /// <param name="context">Database context for accessing reservations and stock.</param>
        /// <param name="historyService">Service for logging history events.</param>
        public ReservationCleanupService(
            BiblioMateDbContext context,
            HistoryService historyService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        /// <summary>
        /// Cleans up reservations that have been available for longer than <see cref="ExpirationWindowHours"/> hours.
        /// Restores stock, logs an expiration event for each reservation, removes them, and returns the number removed.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The number of expired reservations that were removed.</returns>
        public async Task<int> CleanupExpiredReservationsAsync(
            CancellationToken cancellationToken = default)
        {
            var expirationThreshold = DateTime.UtcNow.AddHours(-ExpirationWindowHours);

            var expired = await _context.Reservations
                .Where(r =>
                    r.Status == ReservationStatus.Available &&
                    r.AvailableAt.HasValue &&
                    r.AvailableAt.Value <= expirationThreshold)
                .ToListAsync(cancellationToken);

            foreach (var reservation in expired)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reservation.AssignedStockId.HasValue)
                {
                    await RestoreStockAsync(reservation.AssignedStockId.Value, cancellationToken);
                }

                await _historyService.LogEventAsync(
                    userId:       reservation.UserId,
                    eventType:    "ReservationExpired",
                    reservationId: reservation.ReservationId,
                    cancellationToken: cancellationToken);

                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return expired.Count;
        }

        /// <summary>
        /// Restores the stock record with the specified identifier to available.
        /// </summary>
        /// <param name="stockId">Identifier of the stock to restore.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        private async Task RestoreStockAsync(int stockId, CancellationToken cancellationToken)
        {
            await _context.Stocks
                .Where(s => s.StockId == stockId)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.IsAvailable, true), cancellationToken);
        }
    }
}