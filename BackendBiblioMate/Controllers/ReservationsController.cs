using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Loans;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing book reservations.
    /// Users may create and manage their own reservations,
    /// while Librarians and Admins have broader access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        /// Retrieves all reservations (Admins &amp; Librarians only).
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="ReservationReadDto"/>.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReservationReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetReservations(
            CancellationToken cancellationToken = default)
        {
            var list = await _svc.GetAllAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves active reservations for a specific user.
        /// </summary>
        /// <param name="id">Identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="ReservationReadDto"/>,
        /// <c>403 Forbidden</c> if unauthorized.
        /// </returns>
        [Authorize]
        [HttpGet("user/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ReservationReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetUserReservations(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (id != me && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var list = await _svc.GetByUserAsync(id, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves pending reservations for a specific book.
        /// </summary>
        /// <param name="id">Identifier of the book.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of pending <see cref="ReservationReadDto"/>.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet("book/{id}/pending")]
        [ProducesResponseType(typeof(IEnumerable<ReservationReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetPendingForBook(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var list = await _svc.GetPendingForBookAsync(id, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a single reservation by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the reservation.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="ReservationReadDto"/>,
        /// <c>404 NotFound</c>,
        /// <c>403 Forbidden</c> if unauthorized.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReservationReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ReservationReadDto>> GetReservation(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null) return NotFound();

            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.UserId != me && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        /// <param name="dto">Data for the new reservation.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="ReservationReadDto"/>,
        /// <c>403 Forbidden</c> if unauthorized.
        /// </returns>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        [ProducesResponseType(typeof(ReservationReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ReservationReadDto>> CreateReservation(
            [FromBody] ReservationCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.UserId != me) return Forbid();

            var created = await _svc.CreateAsync(dto, me, cancellationToken);
            return CreatedAtAction(
                nameof(GetReservation),
                new { id = created.ReservationId },
                created
            );
        }

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        /// <param name="id">Identifier of the reservation to update.</param>
        /// <param name="dto">Updated reservation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if IDs mismatch;
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReservation(
            [FromRoute] int id,
            [FromBody] ReservationUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ReservationId) return BadRequest();

            var ok = await _svc.UpdateAsync(dto, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a reservation.
        /// </summary>
        /// <param name="id">Identifier of the reservation to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if not found;
        /// <c>403 Forbidden</c> if unauthorized.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteReservation(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null) return NotFound();

            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.UserId != me && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var ok = await _svc.DeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Purges expired reservations (>48h after availability), restores stock, and logs each removal.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a message indicating the number removed.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPost("cleanup-expired")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CleanupExpiredReservations(
            CancellationToken cancellationToken = default)
        {
            var count = await _cleanup.CleanupExpiredReservationsAsync(cancellationToken);
            return Ok(new { message = $"{count} expired reservations removed." });
        }
    }
}