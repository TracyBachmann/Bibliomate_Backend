using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Services.Users
{
    /// <summary>
    /// Implements <see cref="IHistoryService"/> using EF Core to persist
    /// and retrieve history events from the relational database.
    /// </summary>
    public class HistoryService : IHistoryService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="HistoryService"/>.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        public HistoryService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Logs a user history event with optional loan or reservation reference.
        /// </summary>
        /// <param name="userId">Identifier of the user who performed the action.</param>
        /// <param name="eventType">Type of event (e.g., "Loan", "Return", etc.).</param>
        /// <param name="loanId">Optional loan identifier related to the event.</param>
        /// <param name="reservationId">Optional reservation identifier related to the event.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>Asynchronous task representing the logging operation.</returns>
        public async Task LogEventAsync(
            int userId,
            string eventType,
            int? loanId = null,
            int? reservationId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type must be provided.", nameof(eventType));

            var history = new History
            {
                UserId        = userId,
                EventType     = eventType,
                LoanId        = loanId,
                ReservationId = reservationId,
                EventDate     = DateTime.UtcNow
            };

            _context.Histories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a paginated list of history entries for a given user,
        /// ordered by most recent first.
        /// </summary>
        /// <param name="userId">Identifier of the user whose history is retrieved.</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>
        /// List of <see cref="HistoryReadDto"/> representing the user's history page.
        /// </returns>
        public async Task<List<HistoryReadDto>> GetHistoryForUserAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1.");

            return await _context.Histories
                .AsNoTracking()
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
                .ToListAsync(cancellationToken);
        }
    }
}
