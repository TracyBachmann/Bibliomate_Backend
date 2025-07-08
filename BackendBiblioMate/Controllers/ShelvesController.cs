using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing shelves.
    /// Provides CRUD and paginated, zone-filtered endpoints for <see cref="ShelfReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ShelvesController : ControllerBase
    {
        private readonly IShelfService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="ShelvesController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating shelf logic.</param>
        public ShelvesController(IShelfService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all shelves with optional zone filtering and pagination.
        /// </summary>
        [HttpGet, Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all shelves (v1)",
            Description = "Supports optional zone filtering and pagination.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ShelfReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ShelfReadDto>>> GetShelves(
            [FromQuery] int? zoneId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.GetAllAsync(zoneId, page, pageSize, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a specific shelf by its identifier.
        /// </summary>
        [HttpGet("{id}"), Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a shelf by ID (v1)",
            Description = "Returns a single shelf by its identifier.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(typeof(ShelfReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShelfReadDto>> GetShelf(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto is null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new shelf.
        /// </summary>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new shelf (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(typeof(ShelfReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ShelfReadDto>> CreateShelf(
            [FromBody] ShelfCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetShelf), new { id = created.ShelfId }, created);
        }

        /// <summary>
        /// Updates an existing shelf.
        /// </summary>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing shelf (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateShelf(
            [FromRoute] int id,
            [FromBody] ShelfUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ShelfId)
                return BadRequest("Shelf ID in route and payload do not match.");

            var updated = await _service.UpdateAsync(dto, cancellationToken);
            return updated ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a shelf.
        /// </summary>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a shelf (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteShelf(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var deleted = await _service.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
    }
}