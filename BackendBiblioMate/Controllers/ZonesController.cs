using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing library zones (physical areas in which shelves are organized).
    /// Supports paginated queries and full CRUD operations for <see cref="ZoneReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ZonesController : ControllerBase
    {
        private readonly IZoneService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="ZonesController"/>.
        /// </summary>
        public ZonesController(IZoneService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all zones with optional pagination.
        /// </summary>
        [HttpGet, Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all zones (v1)",
            Description = "Returns paginated list of zones.",
            Tags = ["Zones"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ZoneReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ZoneReadDto>>> GetZones(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var list = await _service.GetAllAsync(page, pageSize, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific zone by its identifier.
        /// </summary>
        [HttpGet("{id}"), Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a zone by ID (v1)",
            Description = "Returns zone details by its ID.",
            Tags = ["Zones"]
        )]
        [ProducesResponseType(typeof(ZoneReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZoneReadDto>> GetZone(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new zone.
        /// </summary>
        [HttpPost, Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new zone (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Zones"]
        )]
        [ProducesResponseType(typeof(ZoneReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ZoneReadDto>> CreateZone(
            [FromBody] ZoneCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetZone), new { id = created.ZoneId }, created);
        }

        /// <summary>
        /// Updates an existing zone.
        /// </summary>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing zone (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Zones"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateZone(
            [FromRoute] int id,
            [FromBody] ZoneUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ZoneId)
                return BadRequest("Route ID and payload ZoneId do not match.");

            var ok = await _service.UpdateAsync(id, dto, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a zone.
        /// </summary>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a zone (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Zones"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteZone(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var ok = await _service.DeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
    }
}