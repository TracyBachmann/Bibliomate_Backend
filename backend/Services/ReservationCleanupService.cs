using backend.Data;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Service that purges reservations which have expired (not collected within the allowed window),
    /// restores their stock, logs each expiration, and returns the count of removed reservations.
    /// </summary>
    public class ReservationCleanupService
    {
        private readonly BiblioMateDbContext _context;
        private readonly HistoryService _historyService;

        /// <summary>
        /// Constructs the cleanup service with required dependencies.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="historyService">Service for logging history events.</param>
        public ReservationCleanupService(
            BiblioMateDbContext context,
            HistoryService historyService)    // ← Inject HistoryService
        {
            _context         = context;
            _historyService  = historyService;
        }

        /// <summary>
        /// Finds all reservations that became available more than 48 hours ago,
        /// marks their stock as available again, logs an expiration event for each,
        /// deletes them, and returns the number of deleted reservations.
        /// </summary>
        public async Task<int> CleanupExpiredReservationsAsync()
        {
            var expirationThreshold = DateTime.UtcNow.AddHours(-48);

            var expiredReservations = await _context.Reservations
                .Where(r =>
                    r.Status == ReservationStatus.Available &&
                    r.AvailableAt != null &&
                    r.AvailableAt <= expirationThreshold)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                // Restore stock availability
                if (reservation.AssignedStockId != null)
                {
                    var stock = await _context.Stocks.FindAsync(reservation.AssignedStockId.Value);
                    if (stock != null)
                        stock.IsAvailable = true;
                }

                // Log expiration in user history
                await _historyService.LogEventAsync(
                    reservation.UserId,
                    eventType: "ReservationExpired",
                    reservationId: reservation.ReservationId
                );

                // Remove the expired reservation
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return expiredReservations.Count;
        }
    }
}
