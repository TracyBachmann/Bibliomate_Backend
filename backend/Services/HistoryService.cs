using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class HistoryService
    {
        private readonly BiblioMateDbContext _context;

        public HistoryService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Logs a new history event.
        /// </summary>
        public async Task LogEventAsync(int userId, string eventType, int? loanId = null, int? reservationId = null)
        {
            var history = new History
            {
                UserId = userId,
                EventType = eventType,
                LoanId = loanId,
                ReservationId = reservationId,
                EventDate = DateTime.UtcNow
            };
            _context.Histories.Add(history);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the paged history for a given user.
        /// </summary>
        public async Task<List<HistoryReadDto>> GetHistoryForUserAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Histories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.EventDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HistoryReadDto
                {
                    HistoryId     = h.HistoryId,
                    EventType     = h.EventType,
                    EventDate     = h.EventDate,
                    LoanId        = h.LoanId,
                    ReservationId = h.ReservationId
                })
                .ToListAsync();
        }
    }
}