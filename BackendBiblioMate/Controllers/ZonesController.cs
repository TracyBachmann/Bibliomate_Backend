using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller responsible for managing library zones.
    /// Zones represent physical areas in which shelves are organized.
    /// Provides full CRUD operations and supports pagination on read operations.
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
        /// <param name="service">
        /// The service handling business logic and data access for zones.
        /// </param>
        public ZonesController(IZoneService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all zones with optional pagination.
        /// </summary>
        /// <remarks>
        /// - This endpoint requires authentication.  
        /// - The response is paginated based on <paramref name="page"/> and <paramref name="pageSize"/>.
        /// </remarks>
        /// <param name="page">The page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <param name="cancellationToken">Token to observe for request cancellation.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="ZoneReadDto"/> instances.  
        /// </returns>
        [HttpGet, Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all zones (v1)",
            Description = "Returns a paginated list of zones.",
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
        /// <remarks>
        /// - This endpoint requires authentication.  
        /// - Returns <c>404 Not Found</c> if the zone does not exist.
        /// </remarks>
        /// <param name="id">The unique identifier of the zone.</param>
        /// <param name="cancellationToken">Token to observe for request cancellation.</param>
        /// <returns>
        /// <c>200 OK</c> with the requested <see cref="ZoneReadDto"/>.  
        /// <c>404 Not Found</c> if the zone does not exist.  
        /// </returns>
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
        /// <remarks>
        /// - Accessible only to Librarians and Admins.  
        /// - On success, the created resource is returned and the
        ///   <c>Location</c> header points to the new resource.
        /// </remarks>
        /// <param name="dto">The data used to create the new zone.</param>
        /// <param name="cancellationToken">Token to observe for request cancellation.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="ZoneReadDto"/>.  
        /// <c>401 Unauthorized</c> if the user is not authenticated.  
        /// <c>403 Forbidden</c> if the user lacks required roles.  
        /// </returns>
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
        /// <remarks>
        /// - Accessible only to Librarians and Admins.  
        /// - The route <paramref name="id"/> must match <see cref="ZoneUpdateDto.ZoneId"/> in the payload.  
        /// - Returns <c>404 Not Found</c> if the zone does not exist.  
        /// </remarks>
        /// <param name="id">The identifier of the zone to update.</param>
        /// <param name="dto">The updated zone data.</param>
        /// <param name="cancellationToken">Token to observe for request cancellation.</param>
        /// <returns>
        /// <c>204 No Content</c> if successfully updated.  
        /// <c>400 Bad Request</c> if the IDs do not match.  
        /// <c>404 Not Found</c> if the zone does not exist.  
        /// <c>401 Unauthorized</c> if the user is not authenticated.  
        /// <c>403 Forbidden</c> if the user lacks required roles.  
        /// </returns>
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
        /// <remarks>
        /// - Accessible only to Librarians and Admins.  
        /// - Permanently removes the zone from the system.  
        /// </remarks>
        /// <param name="id">The identifier of the zone to delete.</param>
        /// <param name="cancellationToken">Token to observe for request cancellation.</param>
        /// <returns>
        /// <c>204 No Content</c> if successfully deleted.  
        /// <c>404 Not Found</c> if the zone does not exist.  
        /// <c>401 Unauthorized</c> if the user is not authenticated.  
        /// <c>403 Forbidden</c> if the user lacks required roles.  
        /// </returns>
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
