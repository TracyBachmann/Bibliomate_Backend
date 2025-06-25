using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Mongo;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing book reservations.
    /// Users may create and manage their own reservations,  
    /// while Librarians and Admins have broader access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly HistoryService _historyService;
        private readonly UserActivityLogService _activityLog;    // audit log service

        /// <summary>
        /// Constructor with DI for DbContext, services and audit log.
        /// </summary>
        /// <param name="context">Database context for BiblioMate.</param>
        /// <param name="historyService">Service to record history events.</param>
        /// <param name="activityLog">Service to record audit logs in MongoDB.</param>
        public ReservationsController(
            BiblioMateDbContext context,
            HistoryService historyService,
            UserActivityLogService activityLog)
        {
            _context        = context;
            _historyService = historyService;
            _activityLog    = activityLog;
        }

        /// <summary>
        /// Retrieves all reservations.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <returns>A list of <see cref="ReservationReadDto"/>.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync();

            return Ok(reservations.Select(r => new ReservationReadDto
            {
                ReservationId   = r.ReservationId,
                UserId          = r.UserId,
                UserName        = r.User.Name,
                BookId          = r.BookId,
                BookTitle       = r.Book.Title,
                ReservationDate = r.ReservationDate,
                Status          = r.Status
            }));
        }

        /// <summary>
        /// Retrieves active reservations for a specific user.
        /// </summary>
        /// <param name="id">The user's identifier.</param>
        /// <returns>A list of <see cref="ReservationReadDto"/>.</returns>
        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetUserReservations(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id != currentUserId 
                && !User.IsInRole(UserRoles.Admin) 
                && !User.IsInRole(UserRoles.Librarian))
            {
                return Forbid();
            }

            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.UserId == id 
                            && (r.Status == ReservationStatus.Pending 
                                || r.Status == ReservationStatus.Available))
                .ToListAsync();

            var result = reservations
                .Select(r => new ReservationReadDto
                {
                    ReservationId   = r.ReservationId,
                    UserId          = r.UserId,
                    UserName        = r.User.Name,
                    BookId          = r.BookId,
                    BookTitle       = r.Book.Title,
                    ReservationDate = r.ReservationDate,
                    Status          = r.Status
                });

            return Ok(result);
        }

        /// <summary>
        /// Retrieves pending reservations for a specific book.
        /// </summary>
        /// <param name="id">The book’s identifier.</param>
        /// <returns>A list of <see cref="ReservationReadDto"/>.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet("book/{id}/pending")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetPendingReservationsForBook(int id)
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == id && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            var result = reservations.Select(r => new ReservationReadDto
            {
                ReservationId   = r.ReservationId,
                UserId          = r.UserId,
                UserName        = r.User.Name,
                BookId          = r.BookId,
                BookTitle       = r.Book.Title,
                ReservationDate = r.ReservationDate,
                Status          = r.Status
            });

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific reservation by its identifier.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <returns>The <see cref="ReservationReadDto"/> if found; otherwise <c>404</c>.</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationReadDto>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            var currentUserId  = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isOwner        = reservation.UserId == currentUserId;
            var isAdminOrStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

            if (!isOwner && !isAdminOrStaff)
                return Forbid();

            return Ok(new ReservationReadDto
            {
                ReservationId   = reservation.ReservationId,
                UserId          = reservation.UserId,
                UserName        = reservation.User.Name,
                BookId          = reservation.BookId,
                BookTitle       = reservation.Book.Title,
                ReservationDate = reservation.ReservationDate,
                Status          = reservation.Status
            });
        }

        /// <summary>
        /// Creates a new reservation for the currently authenticated user.
        /// </summary>
        /// <param name="dto">Data for creating the reservation.</param>
        /// <returns>
        /// <c>201 Created</c> with <see cref="ReservationReadDto"/>; 
        /// <c>400 BadRequest</c> if invalid.
        /// </returns>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        public async Task<ActionResult<ReservationReadDto>> CreateReservation(ReservationCreateDto dto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (dto.UserId != currentUserId)
                return Forbid();

            var hasActiveReservation = await _context.Reservations
                .AnyAsync(r => r.UserId == dto.UserId && 
                               r.BookId == dto.BookId && 
                               r.Status != ReservationStatus.Completed);

            if (hasActiveReservation)
                return BadRequest("You already have an active reservation for this book.");

            var availableStock = await _context.Stocks
                .AnyAsync(s => s.BookId == dto.BookId && s.IsAvailable);

            if (!availableStock)
                return BadRequest("No copies are currently available.");

            var reservation = new Reservation
            {
                UserId          = dto.UserId,
                BookId          = dto.BookId,
                ReservationDate = DateTime.UtcNow,
                Status          = ReservationStatus.Pending,
                CreatedAt       = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Log the reservation event in history
            await _historyService.LogEventAsync(
                reservation.UserId,
                eventType: "Reservation",
                reservationId: reservation.ReservationId
            );

            // Audit log for activity
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = reservation.UserId,
                Action  = "CreateReservation",
                Details = $"ReservationId={reservation.ReservationId}, BookId={reservation.BookId}"
            });

            return CreatedAtAction(nameof(GetReservation),
                new { id = reservation.ReservationId },
                new ReservationReadDto
                {
                    ReservationId   = reservation.ReservationId,
                    UserId          = reservation.UserId,
                    UserName        = (await _context.Users.FindAsync(reservation.UserId))?.Name ?? string.Empty,
                    BookId          = reservation.BookId,
                    BookTitle       = (await _context.Books.FindAsync(reservation.BookId))?.Title ?? string.Empty,
                    ReservationDate = reservation.ReservationDate,
                    Status          = reservation.Status
                });
        }

        /// <summary>
        /// Updates an existing reservation.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <param name="dto">Updated reservation data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success; 
        /// <c>400 BadRequest</c> if mismatch; 
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, ReservationUpdateDto dto)
        {
            if (id != dto.ReservationId)
                return BadRequest();

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.UserId          = dto.UserId;
            reservation.BookId          = dto.BookId;
            reservation.ReservationDate = dto.ReservationDate;
            reservation.Status          = dto.Status;

            await _context.SaveChangesAsync();

            // Audit log for activity
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = reservation.UserId,
                Action  = "UpdateReservation",
                Details = $"ReservationId={reservation.ReservationId}, Status={reservation.Status}"
            });

            return NoContent();
        }

        /// <summary>
        /// Deletes a reservation.
        /// Allowed to the reservation’s owner, Librarians, or Admins.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success; 
        /// <c>404 NotFound</c> if not found; 
        /// <c>403 Forbid</c> if unauthorized.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            var currentUserId  = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isOwner        = reservation.UserId == currentUserId;
            var isAdminOrStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

            if (!isOwner && !isAdminOrStaff)
                return Forbid();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            // Audit log for activity
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = currentUserId,
                Action  = "DeleteReservation",
                Details = $"ReservationId={reservation.ReservationId}"
            });

            return NoContent();
        }
        
        /// <summary>
        /// Purges expired reservations (e.g. older than 48h after availability).
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="cleanupService">Injected cleanup service.</param>
        /// <returns>
        /// <c>200 OK</c> with count of removed reservations.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost("cleanup-expired")]
        public async Task<IActionResult> CleanupExpiredReservations([FromServices] ReservationCleanupService cleanupService)
        {
            int count = await cleanupService.CleanupExpiredReservationsAsync();

            // Audit log for activity
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = 0,
                Action  = "CleanupExpiredReservations",
                Details = $"Count={count}"
            });

            return Ok(new { message = $"{count} expired reservations removed." });
        }
    }
}
