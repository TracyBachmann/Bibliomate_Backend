using backend.Data;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class ReservationCleanupService
    {
        private readonly BiblioMateDbContext _context;

        public ReservationCleanupService(BiblioMateDbContext context)
        {
            _context = context;
        }

        public async Task<int> CleanupExpiredReservationsAsync()
        {
            var expirationThreshold = DateTime.UtcNow.AddHours(-48);

            var expiredReservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Available &&
                            r.AvailableAt != null &&
                            r.AvailableAt <= expirationThreshold)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                if (reservation.AssignedStockId != null)
                {
                    var stock = await _context.Stocks.FindAsync(reservation.AssignedStockId.Value);
                    if (stock != null)
                        stock.IsAvailable = true;
                }

                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return expiredReservations.Count;
        }
    }
}