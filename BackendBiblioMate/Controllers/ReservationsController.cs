using System.Dynamic;
using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing book reservations.
    /// Users may create and manage their own reservations,
    /// while Librarians and Admins have broader access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _svc;
        private readonly IReservationCleanupService _cleanup;

        public ReservationsController(
            IReservationService svc,
            IReservationCleanupService cleanup)
        {
            _svc     = svc;
            _cleanup = cleanup;
        }

        // ---------- helpers ----------
        private int? TryGetUserIdFromClaims()
        {
            var val =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub") ??
                User.FindFirstValue("nameid");
            return int.TryParse(val, out var id) ? id : null;
        }

        /// <summary>
        /// Retrieves all reservations.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="ReservationReadDto"/>.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not Admin or Librarian.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all reservations (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservationReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetReservations(
            CancellationToken cancellationToken = default)
        {
            var list = await _svc.GetAllAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves active reservations for a specific user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="ReservationReadDto"/>.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the caller has no valid identity.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not the owner, Librarian, or Admin.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet("user/{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves active reservations for a user (v1)",
            Description = "Users can view their own reservations. Admins and Librarians can view any user's reservations.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservationReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetUserReservations(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var me = TryGetUserIdFromClaims();
            if (me == null) return Unauthorized();

            if (id != me.Value && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var list = await _svc.GetByUserAsync(id, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves pending reservations for a specific book.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="ReservationReadDto"/>.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not Admin or Librarian.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet("book/{id}/pending")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves pending reservations for a book (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservationReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        /// <param name="id">The reservation identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the reservation details.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the caller has no valid identity.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not the owner, Librarian, or Admin.</description></item>
        /// <item><description><c>404 NotFound</c> if the reservation does not exist.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a reservation by ID (v1)",
            Description = "Enforces ownership or Admin/Librarian access.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(ReservationReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservationReadDto>> GetReservation(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var me = TryGetUserIdFromClaims();
            if (me == null) return Unauthorized();

            if (dto.UserId != me.Value && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        /// <param name="dto">The reservation creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>201 Created</c> with the created reservation.</description></item>
        /// <item><description><c>400 BadRequest</c> if the request is invalid.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the caller has no valid identity.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller tries to create a reservation for another user.</description></item>
        /// <item><description><c>409 Conflict</c> if an active reservation already exists.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new reservation (v1)",
            Description = "Users can create their own reservations.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(ReservationReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ReservationReadDto>> CreateReservation(
            [FromBody] ReservationCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var me = TryGetUserIdFromClaims();
            if (me == null) return Unauthorized();

            if (dto.UserId != me.Value)
                return Forbid();

            try
            {
                var created = await _svc.CreateAsync(dto, me.Value, cancellationToken);
                return CreatedAtAction(
                    nameof(GetReservation),
                    new { id = created.ReservationId },
                    created);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                var msg = ex.Message;

                if (msg.Contains("Existing active reservation", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { error = "ReservationExists", details = "Réservation déjà enregistrée." });

                if (msg.Contains("Copies available", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { error = "CopiesAvailable", details = "Des exemplaires sont disponibles : empruntez ce livre." });

                return BadRequest(new { error = "InvalidOperation", details = msg });
            }
        }

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <param name="dto">The updated reservation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> if the reservation was updated successfully.</description></item>
        /// <item><description><c>400 BadRequest</c> if IDs mismatch.</description></item>
        /// <item><description><c>404 NotFound</c> if the reservation does not exist.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing reservation (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReservation(
            [FromRoute] int id,
            [FromBody] ReservationUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ReservationId)
                return BadRequest();

            var ok = await _svc.UpdateAsync(dto, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a reservation.
        /// </summary>
        /// <param name="id">The reservation identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> if the reservation was deleted successfully.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the caller has no valid identity.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not the owner, Librarian, or Admin.</description></item>
        /// <item><description><c>404 NotFound</c> if the reservation does not exist.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a reservation (v1)",
            Description = "Users can delete their own reservations. Admins and Librarians can delete any.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReservation(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var me = TryGetUserIdFromClaims();
            if (me == null) return Unauthorized();

            if (dto.UserId != me.Value && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var ok = await _svc.DeleteAsync(id, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Purges expired reservations (>48h after availability), restores stock, and logs each removal.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the number of expired reservations removed.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not Admin or Librarian.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPost("cleanup-expired")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Cleans up expired reservations (v1)",
            Description = "Removes expired reservations, restores stock, and logs the removals. Admins and Librarians only.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CleanupExpiredReservations(
            CancellationToken cancellationToken = default)
        {
            var count = await _cleanup.CleanupExpiredReservationsAsync(cancellationToken);
            dynamic ok = new ExpandoObject();
            ok.message = $"{count} expired reservations removed.";
            return Ok(ok);
        }
    }
}
