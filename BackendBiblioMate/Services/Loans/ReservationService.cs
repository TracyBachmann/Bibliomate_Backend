using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Default implementation of <see cref="IReservationService"/>.
    /// Handles reservations using EF Core data access and coordinates history and audit logging.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly BiblioMateDbContext _context;
        private readonly IHistoryService _history;
        private readonly IUserActivityLogService _audit;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationService"/> class.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        /// <param name="historyService">The service used to log historical reservation events.</param>
        /// <param name="activityLogService">The service used to log user activity for auditing.</param>
        public ReservationService(
            BiblioMateDbContext context,
            IHistoryService historyService,
            IUserActivityLogService activityLogService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _history = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _audit   = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        /// <summary>
        /// Retrieves all reservations, including their associated user and book details.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A collection of <see cref="ReservationReadDto"/> objects representing all reservations.</returns>
        public async Task<IEnumerable<ReservationReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves all active or available reservations for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A collection of <see cref="ReservationReadDto"/> for the given user.</returns>
        public async Task<IEnumerable<ReservationReadDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var entities = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.UserId == userId &&
                            (r.Status == ReservationStatus.Pending ||
                             r.Status == ReservationStatus.Available))
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves all pending reservations for a specific book, ordered by creation date.
        /// </summary>
        /// <param name="bookId">The identifier of the book.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A collection of pending <see cref="ReservationReadDto"/> for the specified book.</returns>
        public async Task<IEnumerable<ReservationReadDto>> GetPendingForBookAsync(int bookId, CancellationToken cancellationToken = default)
        {
            var entities = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves a single reservation by its identifier.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="ReservationReadDto"/> if found; otherwise <c>null</c>.</returns>
        public async Task<ReservationReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == id, cancellationToken);

            return entity is null ? null : MapToDto(entity);
        }

        /// <summary>
        /// Creates a new reservation if the user is authorized and no copies are currently available.
        /// </summary>
        /// <param name="dto">The reservation creation data transfer object.</param>
        /// <param name="currentUserId">The identifier of the currently authenticated user.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>The newly created <see cref="ReservationReadDto"/>.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user ID in the request does not match the authenticated user.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user already has an active reservation or if copies are available.</exception>
        public async Task<ReservationReadDto> CreateAsync(ReservationCreateDto dto, int currentUserId, CancellationToken cancellationToken = default)
        {
            if (dto.UserId != currentUserId)
                throw new UnauthorizedAccessException("User mismatch.");

            // Rule 1: Prevent duplicate active reservations for the same book.
            var hasActive = await _context.Reservations
                .AnyAsync(r => r.UserId == dto.UserId &&
                               r.BookId == dto.BookId &&
                               r.Status != ReservationStatus.Completed,
                          cancellationToken);
            if (hasActive)
                throw new InvalidOperationException("Existing active reservation for this book.");

            // Rule 2: Ensure no copies are available before allowing a reservation.
            var stockQty = await _context.Stocks
                .Where(s => s.BookId == dto.BookId)
                .Select(s => (int?)s.Quantity)
                .FirstOrDefaultAsync(cancellationToken) ?? 0;

            var activeLoans = await _context.Loans
                .CountAsync(l => l.BookId == dto.BookId && l.ReturnDate == null, cancellationToken);

            var remaining = stockQty - activeLoans;
            if (remaining > 0)
                throw new InvalidOperationException("Copies available. Please borrow instead of reserving.");

            // Rule 3: Create the reservation.
            var now = DateTime.UtcNow;
            var entity = new Reservation
            {
                UserId          = dto.UserId,
                BookId          = dto.BookId,
                ReservationDate = now,
                Status          = ReservationStatus.Pending,
                CreatedAt       = now
            };

            _context.Reservations.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await LogHistoryAndAuditAsync(
                userId: entity.UserId,
                action: "CreateReservation",
                reservationId: entity.ReservationId,
                cancellationToken: cancellationToken);

            // Reload entity to hydrate navigation properties.
            entity = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstAsync(r => r.ReservationId == entity.ReservationId, cancellationToken);

            return MapToDto(entity);
        }

        /// <summary>
        /// Updates an existing reservation’s details.
        /// </summary>
        /// <param name="dto">The reservation update data transfer object.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns><c>true</c> if the reservation was updated successfully; otherwise <c>false</c>.</returns>
        public async Task<bool> UpdateAsync(ReservationUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == dto.ReservationId, cancellationToken);
            if (entity is null)
                return false;

            entity.UserId          = dto.UserId;
            entity.BookId          = dto.BookId;
            entity.ReservationDate = dto.ReservationDate;
            entity.Status          = dto.Status;

            await _context.SaveChangesAsync(cancellationToken);

            await LogAuditAsync(
                userId: dto.UserId,
                action: "UpdateReservation",
                details: $"ReservationId={dto.ReservationId}, Status={dto.Status}",
                cancellationToken: cancellationToken);

            return true;
        }

        /// <summary>
        /// Deletes a reservation by its identifier.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns><c>true</c> if the reservation was deleted; otherwise <c>false</c>.</returns>
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Reservations
                .FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
                return false;

            _context.Reservations.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await LogAuditAsync(
                userId: entity.UserId,
                action: "DeleteReservation",
                details: $"ReservationId={entity.ReservationId}",
                cancellationToken: cancellationToken);

            return true;
        }

        // ----------------- Helper methods -----------------

        /// <summary>
        /// Logs both a history event and a user activity audit entry.
        /// </summary>
        /// <param name="userId">The user who performed the action.</param>
        /// <param name="action">The action performed (e.g., CreateReservation).</param>
        /// <param name="reservationId">The identifier of the reservation affected.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        private async Task LogHistoryAndAuditAsync(int userId, string action, int reservationId, CancellationToken cancellationToken)
        {
            await _history.LogEventAsync(
                userId: userId,
                eventType: "Reservation",
                reservationId: reservationId,
                cancellationToken: cancellationToken);

            await _audit.LogAsync(
                new UserActivityLogDocument
                {
                    UserId  = userId,
                    Action  = action,
                    Details = $"ReservationId={reservationId}"
                },
                cancellationToken);
        }

        /// <summary>
        /// Logs a user activity audit entry only (without creating a history event).
        /// </summary>
        /// <param name="userId">The user who performed the action.</param>
        /// <param name="action">The action performed.</param>
        /// <param name="details">Additional details for the log entry.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        private async Task LogAuditAsync(int userId, string action, string details, CancellationToken cancellationToken)
        {
            await _audit.LogAsync(
                new UserActivityLogDocument
                {
                    UserId  = userId,
                    Action  = action,
                    Details = details
                },
                cancellationToken);
        }

        /// <summary>
        /// Maps a <see cref="Reservation"/> entity to its <see cref="ReservationReadDto"/>.
        /// </summary>
        /// <param name="r">The reservation entity to map.</param>
        /// <returns>A corresponding <see cref="ReservationReadDto"/>.</returns>
        private static ReservationReadDto MapToDto(Reservation r) => new()
        {
            ReservationId   = r.ReservationId,
            UserId          = r.UserId,
            UserName        = r.User != null ? $"{r.User.FirstName} {r.User.LastName}".Trim() : string.Empty,
            BookId          = r.BookId,
            BookTitle       = r.Book?.Title ?? string.Empty,
            ReservationDate = r.ReservationDate,
            Status          = r.Status,
            ExpirationDate  = r.AvailableAt.HasValue 
                ? r.AvailableAt.Value.AddHours(48)   // Business rule: 48h after availability
                : (DateTime?)null
        };
    }
}
