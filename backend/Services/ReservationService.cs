using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Mongo;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Default implementation of <see cref="IReservationService"/>,
    /// handling EF Core data access, history logging, and audit logging.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly BiblioMateDbContext _context;
        private readonly HistoryService _history;
        private readonly UserActivityLogService _audit;

        public ReservationService(
            BiblioMateDbContext context,
            HistoryService historyService,
            UserActivityLogService activityLog)
        {
            _context = context;
            _history = historyService;
            _audit   = activityLog;
        }

        public async Task<IEnumerable<ReservationReadDto>> GetAllAsync()
        {
            var list = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync();

            return list.Select(ToDto);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetByUserAsync(int userId)
        {
            var list = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r =>
                    r.UserId == userId &&
                    (r.Status == ReservationStatus.Pending ||
                     r.Status == ReservationStatus.Available))
                .ToListAsync();

            return list.Select(ToDto);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetPendingForBookAsync(int bookId)
        {
            var list = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return list.Select(ToDto);
        }

        public async Task<ReservationReadDto?> GetByIdAsync(int id)
        {
            var r = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            return r == null ? null : ToDto(r);
        }

        public async Task<ReservationReadDto> CreateAsync(ReservationCreateDto dto, int currentUserId)
        {
            if (dto.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            var hasActive = await _context.Reservations.AnyAsync(r =>
                r.UserId == dto.UserId &&
                r.BookId == dto.BookId &&
                r.Status != ReservationStatus.Completed);

            if (hasActive)
                throw new InvalidOperationException("You already have an active reservation for this book.");

            var avail = await _context.Stocks.AnyAsync(s =>
                s.BookId == dto.BookId && s.IsAvailable);

            if (!avail)
                throw new InvalidOperationException("No copies are currently available.");

            var entity = new Reservation
            {
                UserId          = dto.UserId,
                BookId          = dto.BookId,
                ReservationDate = DateTime.UtcNow,
                Status          = ReservationStatus.Pending,
                CreatedAt       = DateTime.UtcNow
            };

            _context.Reservations.Add(entity);
            await _context.SaveChangesAsync();

            await _history.LogEventAsync(entity.UserId, "Reservation", reservationId: entity.ReservationId);
            await _audit.LogAsync(new UserActivityLogDocument {
                UserId  = entity.UserId,
                Action  = "CreateReservation",
                Details = $"ReservationId={entity.ReservationId}, BookId={entity.BookId}"
            });

            var loaded = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstAsync(r => r.ReservationId == entity.ReservationId);

            return ToDto(loaded);
        }

        public async Task<bool> UpdateAsync(ReservationUpdateDto dto)
        {
            var entity = await _context.Reservations.FindAsync(dto.ReservationId);
            if (entity == null) return false;

            entity.UserId          = dto.UserId;
            entity.BookId          = dto.BookId;
            entity.ReservationDate = dto.ReservationDate;
            entity.Status          = dto.Status;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(new UserActivityLogDocument
            {
                UserId  = entity.UserId,
                Action  = "UpdateReservation",
                Details = $"ReservationId={entity.ReservationId}, Status={entity.Status}"
            });

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Reservations.FindAsync(id);
            if (entity == null) return false;

            _context.Reservations.Remove(entity);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(new UserActivityLogDocument
            {
                UserId  = entity.UserId,
                Action  = "DeleteReservation",
                Details = $"ReservationId={entity.ReservationId}"
            });

            return true;
        }

        private static ReservationReadDto ToDto(Reservation r)
            => new()
            {
                ReservationId   = r.ReservationId,
                UserId          = r.UserId,
                UserName        = r.User?.Name ?? string.Empty,
                BookId          = r.BookId,
                BookTitle       = r.Book?.Title ?? string.Empty,
                ReservationDate = r.ReservationDate,
                Status          = r.Status
            };
    }
}