using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
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
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync();
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
        public async Task<ActionResult<Reservation>> GetReservation(int id)
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

            return reservation;
        }

        // POST: api/Reservations
        /// <summary>
        /// Creates a new reservation for the currently authenticated user.
        /// </summary>
        /// <param name="reservation">The reservation entity to create (BookId required).</param>
        /// <returns>
        /// <c>201 Created</c> with the created reservation and its URI;  
        /// <c>403 Forbid</c> if attempting to create for another user.
        /// </returns>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (reservation.UserId != currentUserId)
                return Forbid();

            reservation.ReservationDate = DateTime.UtcNow;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation),
                new { id = reservation.ReservationId }, reservation);
        }

        // PUT: api/Reservations/{id}
        /// <summary>
        /// Updates an existing reservation.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the reservation to update.</param>
        /// <param name="reservation">The modified reservation entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if the reservation does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId)
                return BadRequest();

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservations.Any(r => r.ReservationId == id))
                    return NotFound();
                throw;
            }

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
