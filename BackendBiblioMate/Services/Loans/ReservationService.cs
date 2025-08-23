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
    /// Default implementation of <see cref="IReservationService"/>,
    /// handling EF Core data access, history logging, and audit logging.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly BiblioMateDbContext _context;
        private readonly IHistoryService _history;
        private readonly IUserActivityLogService _audit;

        /// <summary>
        /// Initializes a new instance of <see cref="ReservationService"/>.
        /// </summary>
        /// <param name="context">Database context for reservations.</param>
        /// <param name="historyService">Service for logging history events.</param>
        /// <param name="activityLogService">Service for logging user activity.</param>
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
        /// Retrieves all reservations with associated user and book details.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>List of <see cref="ReservationReadDto"/>.</returns>
        public async Task<IEnumerable<ReservationReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var entities = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves active or available reservations for a specific user.
        /// </summary>
        /// <param name="userId">Identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>List of <see cref="ReservationReadDto"/>.</returns>
        public async Task<IEnumerable<ReservationReadDto>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
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
        /// Retrieves pending reservations for a given book, ordered by creation time.
        /// </summary>
        /// <param name="bookId">Identifier of the book.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>List of <see cref="ReservationReadDto"/>.</returns>
        public async Task<IEnumerable<ReservationReadDto>> GetPendingForBookAsync(
            int bookId,
            CancellationToken cancellationToken = default)
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
        /// Retrieves a reservation by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the reservation.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>
        /// The <see cref="ReservationReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<ReservationReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == id, cancellationToken);

            return entity is null
                ? null
                : MapToDto(entity);
        }

        /// <summary>
        /// Creates a new reservation if the user is authorized and a copy is available.
        /// </summary>
        /// <param name="dto">Data transfer object containing reservation details.</param>
        /// <param name="currentUserId">Identifier of the user performing the operation.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>The created <see cref="ReservationReadDto"/>.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user ID does not match.</exception>
        /// <exception cref="InvalidOperationException">Thrown if reservation rules are violated.</exception>
        public async Task<ReservationReadDto> CreateAsync(
            ReservationCreateDto dto,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (dto.UserId != currentUserId)
                throw new UnauthorizedAccessException("User mismatch.");

            var hasActive = await _context.Reservations
                .AnyAsync(r => r.UserId == dto.UserId &&
                               r.BookId == dto.BookId &&
                               r.Status != ReservationStatus.Completed,
                          cancellationToken);
            if (hasActive)
                throw new InvalidOperationException("Existing active reservation for this book.");

            var available = await _context.Stocks
                .AnyAsync(s => s.BookId == dto.BookId && s.Quantity > 0, cancellationToken);
            if (!available)
                throw new InvalidOperationException("No copies available.");

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

            // Reload to get navigation properties
            entity = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstAsync(r => r.ReservationId == entity.ReservationId, cancellationToken);

            return MapToDto(entity);
        }

        /// <summary>
        /// Updates an existing reservation’s details.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated reservation data.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns><c>true</c> if update succeeded; <c>false</c> if not found.</returns>
        public async Task<bool> UpdateAsync(
            ReservationUpdateDto dto,
            CancellationToken cancellationToken = default)
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
        /// <param name="id">Identifier of the reservation to delete.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns><c>true</c> if deletion succeeded; <c>false</c> if not found.</returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Reservations
                .FindAsync(new object[]{ id }, cancellationToken);
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

        /// <summary>
        /// Logs a history event and an audit record for the reservation operation.
        /// </summary>
        /// <param name="userId">Identifier of the user performing the action.</param>
        /// <param name="action">Action name to record.</param>
        /// <param name="reservationId">Identifier of the reservation.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        private async Task LogHistoryAndAuditAsync(
            int userId,
            string action,
            int reservationId,
            CancellationToken cancellationToken)
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
        /// Logs only an audit record without a history event.
        /// </summary>
        /// <param name="userId">Identifier of the user performing the action.</param>
        /// <param name="action">Action name to record.</param>
        /// <param name="details">Details to include in the log.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        private async Task LogAuditAsync(
            int userId,
            string action,
            string details,
            CancellationToken cancellationToken)
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
        /// <param name="r">Reservation entity to map.</param>
        /// <returns>A new <see cref="ReservationReadDto"/> instance.</returns>
        private static ReservationReadDto MapToDto(Reservation r) => new()
        {
            ReservationId   = r.ReservationId,
            UserId          = r.UserId,
            // Combine first/last name; Trim() avoids trailing space if one part is empty.
            UserName        = r.User != null ? $"{r.User.FirstName} {r.User.LastName}".Trim() : string.Empty,
            BookId          = r.BookId,
            BookTitle       = r.Book?.Title ?? string.Empty,
            ReservationDate = r.ReservationDate,
            Status          = r.Status
        };
    }
}