using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Services.Users
{
    /// <summary>
    /// Default implementation of <see cref="IHistoryService"/> that uses EF Core
    /// to log and retrieve user history events (loans, returns, reservations, etc.).
    /// </summary>
    public class HistoryService : IHistoryService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryService"/> class.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        public HistoryService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        /// <summary>
        /// Logs a new user history event in the database.
        /// </summary>
        /// <param name="userId">Identifier of the user who triggered the event.</param>
        /// <param name="eventType">Type of event (e.g., "LoanCreated", "ReservationExpired").</param>
        /// <param name="loanId">Optional loan ID linked to the event.</param>
        /// <param name="reservationId">Optional reservation ID linked to the event.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
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

        /// <inheritdoc />
        /// <summary>
        /// Retrieves paginated user history, sorted by most recent first.
        /// </summary>
        /// <param name="userId">The user identifier whose history to retrieve.</param>
        /// <param name="page">Page number (1-based index).</param>
        /// <param name="pageSize">Number of entries per page.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A list of <see cref="HistoryReadDto"/> entries for the given user.</returns>
        public async Task<List<HistoryReadDto>> GetHistoryForUserAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0) 
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
            if (pageSize <= 0) 
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1.");

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
