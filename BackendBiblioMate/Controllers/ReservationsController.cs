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
    /// Controller for managing book reservations.
    /// Users may create and manage their own reservations,
    /// while Librarians and Admins have broader access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _svc;
        private readonly IReservationCleanupService _cleanup;

        /// <summary>
        /// Constructs a new <see cref="ReservationsController"/>.
        /// </summary>
        /// <param name="svc">Service for reservation operations.</param>
        /// <param name="cleanup">Service to purge expired reservations.</param>
        public ReservationsController(
            IReservationService svc,
            IReservationCleanupService cleanup)
        {
            _svc     = svc;
            _cleanup = cleanup;
        }

        /// <summary>
        /// Retrieves all reservations (Admins &amp; Librarians only).
        /// </summary>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all reservations (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reservations"]
        )]
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
        [Authorize]
        [HttpGet("user/{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves active reservations for a user (v1)",
            Description = "Users can view their own reservations. Admins and Librarians can view any user's reservations.",
            Tags = ["Reservations"]
        )]
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet("book/{id}/pending")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves pending reservations for a book (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reservations"]
        )]
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
        [Authorize]
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a reservation by ID (v1)",
            Description = "Enforces ownership or Admin/Librarian access.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(ReservationReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ReservationReadDto>> GetReservation(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.UserId != me && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new reservation (v1)",
            Description = "Users can create their own reservations.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(ReservationReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ReservationReadDto>> CreateReservation(
            [FromBody] ReservationCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.UserId != me)
                return Forbid();

            var created = await _svc.CreateAsync(dto, me, cancellationToken);
            return CreatedAtAction(
                nameof(GetReservation),
                new { id = created.ReservationId },
                created);
        }

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing reservation (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
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
        [Authorize]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a reservation (v1)",
            Description = "Users can delete their own reservations. Admins and Librarians can delete any.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteReservation(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.UserId != me && !User.IsInRole(UserRoles.Librarian) && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var ok = await _svc.DeleteAsync(id, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Purges expired reservations (>48h after availability), restores stock, and logs each removal.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPost("cleanup-expired")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Cleans up expired reservations (v1)",
            Description = "Removes expired reservations, restores stock, and logs the removals. Admins and Librarians only.",
            Tags = ["Reservations"]
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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