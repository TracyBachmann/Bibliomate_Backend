using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
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

        public ReservationsController(
            BiblioMateDbContext context,
            HistoryService historyService)   // ← Injection of HistoryService
        {
            _context        = context;
            _historyService = historyService;
        }

        // GET: api/Reservations
        /// <summary>
        /// Retrieves all reservations.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <returns>A collection of reservations with related user and book data.</returns>
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

        // GET: api/Reservations/user/{id}
        /// <summary>
        /// Retrieves active reservations for a specific user.
        /// </summary>
        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetUserReservations(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id != currentUserId && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Librarian))
                return Forbid();

            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .Where(r => r.UserId == id && 
                            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Available))
                .ToListAsync();

            return Ok(reservations.Select(r => new ReservationReadDto
            {
                ReservationId   = r.ReservationId,
                UserId          = r.UserId,
                UserName        = r.User?.Name ?? "",
                BookId          = r.BookId,
                BookTitle       = r.Book.Title,
                ReservationDate = r.ReservationDate,
                Status          = r.Status
            }));
        }

        // GET: api/Reservations/book/{id}/pending
        /// <summary>
        /// Retrieves pending reservations for a specific book.
        /// </summary>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet("book/{id}/pending")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetPendingReservationsForBook(int id)
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Where(r => r.BookId == id && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reservations.Select(r => new ReservationReadDto
            {
                ReservationId   = r.ReservationId,
                UserId          = r.UserId,
                UserName        = r.User.Name,
                BookId          = r.BookId,
                BookTitle       = r.Book?.Title ?? "",
                ReservationDate = r.ReservationDate,
                Status          = r.Status
            }));
        }

        // GET: api/Reservations/{id}
        /// <summary>
        /// Retrieves a specific reservation by its identifier.
        /// </summary>
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

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isOwner       = reservation.UserId == currentUserId;
            var isAdminOrStaff= User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

            if (!isOwner && !isAdminOrStaff)
                return Forbid();

            return new ReservationReadDto
            {
                ReservationId   = reservation.ReservationId,
                UserId          = reservation.UserId,
                UserName        = reservation.User.Name,
                BookId          = reservation.BookId,
                BookTitle       = reservation.Book.Title,
                ReservationDate = reservation.ReservationDate,
                Status          = reservation.Status
            };
        }

        // POST: api/Reservations
        /// <summary>
        /// Creates a new reservation for the currently authenticated user.
        /// </summary>
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
                return BadRequest("Vous avez déjà une réservation active pour ce livre.");

            var availableStock = await _context.Stocks
                .AnyAsync(s => s.BookId == dto.BookId && s.IsAvailable);

            if (!availableStock)
                return BadRequest("Aucun exemplaire disponible pour le moment.");

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

            // Log the reservation event
            await _historyService.LogEventAsync(
                reservation.UserId,
                eventType: "Reservation",
                reservationId: reservation.ReservationId
            );

            return CreatedAtAction(nameof(GetReservation),
                new { id = reservation.ReservationId },
                new ReservationReadDto
                {
                    ReservationId   = reservation.ReservationId,
                    UserId          = reservation.UserId,
                    UserName        = (await _context.Users.FindAsync(reservation.UserId))?.Name ?? "",
                    BookId          = reservation.BookId,
                    BookTitle       = (await _context.Books.FindAsync(reservation.BookId))?.Title ?? "",
                    ReservationDate = reservation.ReservationDate,
                    Status          = reservation.Status
                });
        }

        // PUT: api/Reservations/{id}
        /// <summary>
        /// Updates an existing reservation.
        /// Accessible to Librarians and Admins only.
        /// </summary>
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
            return NoContent();
        }

        // DELETE: api/Reservations/{id}
        /// <summary>
        /// Deletes a reservation.
        /// Allowed to the reservation’s owner, Librarians, or Admins.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isOwner       = reservation.UserId == currentUserId;
            var isAdminOrStaff= User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

            if (!isOwner && !isAdminOrStaff)
                return Forbid();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        // POST: api/Reservations/cleanup-expired
        /// <summary>
        /// Purges expired reservations (e.g. older than 48h after AvailableAt).
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="cleanupService">Injected cleanup service.</param>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost("cleanup-expired")]
        public async Task<IActionResult> CleanupExpiredReservations([FromServices] ReservationCleanupService cleanupService)
        {
            int count = await cleanupService.CleanupExpiredReservationsAsync();
            return Ok(new { message = $"{count} réservations expirées supprimées." });
        }
    }
}
