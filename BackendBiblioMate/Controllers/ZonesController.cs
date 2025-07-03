using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing library zones (physical areas in which shelves are organized).
    /// Supports paginated queries and full CRUD operations for <see cref="ZoneReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ZonesController : ControllerBase
    {
        private readonly IZoneService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="ZonesController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating zone logic.</param>
        public ZonesController(IZoneService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all zones with optional pagination.
        /// </summary>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>10</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="ZoneReadDto"/>.
        /// </returns>
        [HttpGet, Authorize]
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
        /// <param name="id">The zone identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="ZoneReadDto"/> if found;  
        /// <c>404 NotFound</c> otherwise.
        /// </returns>
        [HttpGet("{id}"), Authorize]
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
        /// <param name="dto">Zone data to create.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="ZoneReadDto"/> and its URI;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
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
        /// <param name="id">The identifier of the zone to update.</param>
        /// <param name="dto">Updated zone data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if zone not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
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
        /// <param name="id">The identifier of the zone to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if zone not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
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