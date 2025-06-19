using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers
{
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
        /// Retrieves all reservations (Admin and Librarian only).
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync();
        }

        // GET: api/Reservations/5
        /// <summary>
        /// Retrieves a specific reservation if user is owner or staff.
        /// </summary>
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

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isOwner = reservation.UserId == currentUserId;
            var isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");

            if (!isOwner && !isAdminOrStaff)
                return Forbid();

            return reservation;
        }

        // POST: api/Reservations
        /// <summary>
        /// Creates a reservation for the currently authenticated user.
        /// </summary>
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Prevents booking on behalf of another user
            if (reservation.UserId != currentUserId)
                return Forbid();

            reservation.ReservationDate = DateTime.UtcNow;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.ReservationId }, reservation);
        }

        // PUT: api/Reservations/5
        /// <summary>
        /// Updates an existing reservation (Admin and Librarian only).
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Reservations/5
        /// <summary>
        /// Deletes a reservation if user is owner or staff.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isOwner = reservation.UserId == currentUserId;
            var isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");

            if (!isOwner && !isAdminOrStaff)
                return Forbid();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}