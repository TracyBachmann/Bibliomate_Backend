using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing shelves.
    /// Provides CRUD and paginated, zone-filtered endpoints for <see cref="ShelfReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ShelvesController : ControllerBase
    {
        private readonly IShelfService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShelvesController"/> class.
        /// </summary>
        /// <param name="service">Service encapsulating shelf logic.</param>
        public ShelvesController(IShelfService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Retrieves all shelves with optional zone filtering and pagination.
        /// </summary>
        /// <param name="zoneId">Optional zone identifier to filter results.</param>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Number of items per page. Default is <c>10</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="ShelfReadDto"/>.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the user is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all shelves (v1)",
            Description = "Supports optional zone filtering and pagination.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ShelfReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        /// <param name="id">The shelf identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the shelf data.</description></item>
        /// <item><description><c>404 NotFound</c> if the shelf does not exist.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the user is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a shelf by ID (v1)",
            Description = "Returns a single shelf by its identifier.",
            Tags = ["Shelves"]
        )]
        [ProducesResponseType(typeof(ShelfReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        /// Accessible only to Librarians and Admins.
        /// </summary>
        /// <param name="dto">The shelf data to create.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>201 Created</c> with the created <see cref="ShelfReadDto"/>.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the user is not authenticated.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user lacks required roles.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new shelf (v1)",
            Description = "Accessible only to Librarians and Admins.",
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
        /// Accessible only to Librarians and Admins.
        /// </summary>
        /// <param name="id">The shelf identifier.</param>
        /// <param name="dto">The updated shelf data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> on success.</description></item>
        /// <item><description><c>400 BadRequest</c> if the IDs do not match.</description></item>
        /// <item><description><c>404 NotFound</c> if the shelf does not exist.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the user is not authenticated.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user lacks required roles.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing shelf (v1)",
            Description = "Accessible only to Librarians and Admins.",
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
                return BadRequest(new { error = "IdMismatch", details = "Route ID and body ID do not match." });

            var updated = await _service.UpdateAsync(dto, cancellationToken);
            return updated ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a shelf.
        /// Accessible only to Librarians and Admins.
        /// </summary>
        /// <param name="id">The shelf identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> when deletion succeeds.</description></item>
        /// <item><description><c>404 NotFound</c> if the shelf does not exist.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the user is not authenticated.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user lacks required roles.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a shelf (v1)",
            Description = "Accessible only to Librarians and Admins.",
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
