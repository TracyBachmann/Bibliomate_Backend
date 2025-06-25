using System.Security.Claims;
using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IReservationService _svc;
        private readonly ReservationCleanupService _cleanup;

        /// <summary>
        /// Constructs a new <see cref="ReservationsController"/>.
        /// </summary>
        /// <param name="svc">Service for reservation operations.</param>
        /// <param name="cleanup">Service to purge expired reservations.</param>
        public ReservationsController(
            IReservationService svc,
            ReservationCleanupService cleanup)
        {
            _svc     = svc;
            _cleanup = cleanup;
        }

        /// <summary>
        /// GET: api/Reservations
        /// Retrieves all reservations.
        /// </summary>
        /// <remarks>
        /// Accessible only to users in the "Librarian" or "Admin" roles.
        /// </remarks>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="ReservationReadDto"/>.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetReservations()
            => Ok(await _svc.GetAllAsync());

        /// <summary>
        /// GET: api/Reservations/user/{id}
        /// Retrieves active reservations for a specific user.
        /// </summary>
        /// <param name="id">Identifier of the user.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="ReservationReadDto"/>,
        /// or <c>403 Forbid</c> if not authorized.
        /// </returns>
        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetUserReservations(int id)
        {
            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id != me &&
                !User.IsInRole(UserRoles.Librarian) &&
                !User.IsInRole(UserRoles.Admin))
            {
                return Forbid();
            }

            return Ok(await _svc.GetByUserAsync(id));
        }

        /// <summary>
        /// GET: api/Reservations/book/{id}/pending
        /// Retrieves pending reservations for a specific book.
        /// </summary>
        /// <param name="id">Identifier of the book.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of pending <see cref="ReservationReadDto"/>.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet("book/{id}/pending")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetPendingForBook(int id)
            => Ok(await _svc.GetPendingForBookAsync(id));

        /// <summary>
        /// GET: api/Reservations/{id}
        /// Retrieves a single reservation by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the reservation.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="ReservationReadDto"/>,
        /// <c>404 NotFound</c> if not found,
        /// or <c>403 Forbid</c> if unauthorized.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationReadDto>> GetReservation(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();

            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (dto.UserId != me &&
                !User.IsInRole(UserRoles.Librarian) &&
                !User.IsInRole(UserRoles.Admin))
            {
                return Forbid();
            }

            return Ok(dto);
        }

        /// <summary>
        /// POST: api/Reservations
        /// Creates a new reservation.
        /// </summary>
        /// <param name="dto">Data for the new reservation.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="ReservationReadDto"/> and Location header,
        /// or <c>403 Forbid</c> if the userId does not match the authenticated user.
        /// </returns>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        public async Task<ActionResult<ReservationReadDto>> CreateReservation(ReservationCreateDto dto)
        {
            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (dto.UserId != me) return Forbid();

            var created = await _svc.CreateAsync(dto, me);
            return CreatedAtAction(
                nameof(GetReservation),
                new { id = created.ReservationId },
                created
            );
        }

        /// <summary>
        /// PUT: api/Reservations/{id}
        /// Updates an existing reservation.
        /// </summary>
        /// <param name="id">Identifier of the reservation to update.</param>
        /// <param name="dto">Updated reservation data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if the id does not match;
        /// <c>404 NotFound</c> if the reservation does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, ReservationUpdateDto dto)
        {
            if (id != dto.ReservationId) return BadRequest();
            var ok = await _svc.UpdateAsync(dto);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// DELETE: api/Reservations/{id}
        /// Deletes a reservation.
        /// </summary>
        /// <param name="id">Identifier of the reservation to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if missing;
        /// <c>403 Forbid</c> if unauthorized.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();

            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (dto.UserId != me &&
                !User.IsInRole(UserRoles.Librarian) &&
                !User.IsInRole(UserRoles.Admin))
            {
                return Forbid();
            }

            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// POST: api/Reservations/cleanup-expired
        /// Purges reservations that have expired (>48h after availability),
        /// restores stock and logs each removal.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with a message indicating how many were removed.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost("cleanup-expired")]
        public async Task<IActionResult> CleanupExpiredReservations()
        {
            var count = await _cleanup.CleanupExpiredReservationsAsync();
            return Ok(new { message = $"{count} expired reservations removed." });
        }
    }
}