using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Models.Enums;

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

        public ReservationsController(BiblioMateDbContext context)
        {
            _context = context;
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
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                UserName = r.User.Name,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                ReservationDate = r.ReservationDate
            }));
        }

        // GET: api/Reservations/{id}
        /// <summary>
        /// Retrieves a specific reservation by its identifier.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <returns>
        /// The requested reservation if authorized;  
        /// otherwise <c>403 Forbid</c> or <c>404 NotFound</c>.
        /// </returns>
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

            return new ReservationReadDto
            {
                ReservationId = reservation.ReservationId,
                UserId = reservation.UserId,
                UserName = reservation.User.Name,
                BookId = reservation.BookId,
                BookTitle = reservation.Book.Title,
                ReservationDate = reservation.ReservationDate
            };
        }

        // POST: api/Reservations
        /// <summary>
        /// Creates a new reservation for the currently authenticated user.
        /// </summary>
        /// <param name="dto">The reservation entity to create (BookId required).</param>
        /// <returns>
        /// <c>201 Created</c> with the created reservation and its URI;  
        /// <c>403 Forbid</c> if attempting to create for another user.
        /// </returns>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        public async Task<ActionResult<ReservationReadDto>> CreateReservation(ReservationCreateDto dto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (dto.UserId != currentUserId)
                return Forbid();

            var reservation = new Reservation
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                ReservationDate = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation),
                new { id = reservation.ReservationId },
                new ReservationReadDto
                {
                    ReservationId = reservation.ReservationId,
                    UserId = reservation.UserId,
                    UserName = (await _context.Users.FindAsync(reservation.UserId))?.Name ?? "",
                    BookId = reservation.BookId,
                    BookTitle = (await _context.Books.FindAsync(reservation.BookId))?.Title ?? "",
                    ReservationDate = reservation.ReservationDate
                });
        }

        // PUT: api/Reservations/{id}
        /// <summary>
        /// Updates an existing reservation.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the reservation to update.</param>
        /// <param name="dto">The modified reservation entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if the reservation does not exist.
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

            reservation.UserId = dto.UserId;
            reservation.BookId = dto.BookId;
            reservation.ReservationDate = dto.ReservationDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Reservations/{id}
        /// <summary>
        /// Deletes a reservation.
        /// Allowed to the reservation’s owner, Librarians, or Admins.
        /// </summary>
        /// <param name="id">The identifier of the reservation to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>403 Forbid</c> if unauthorized;  
        /// <c>404 NotFound</c> if the reservation is not found.
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

            return NoContent();
        }
    }
}
