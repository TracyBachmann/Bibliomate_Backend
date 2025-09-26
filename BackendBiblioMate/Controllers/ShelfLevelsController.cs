using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing shelf levels.
    /// Provides CRUD operations and paginated queries on <see cref="ShelfLevelReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ShelfLevelsController : ControllerBase
    {
        private readonly IShelfLevelService _svc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShelfLevelsController"/> class.
        /// </summary>
        /// <param name="svc">Service for shelf level operations.</param>
        public ShelfLevelsController(IShelfLevelService svc)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        }

        /// <summary>
        /// Retrieves all shelf levels with optional shelf filtering and pagination.
        /// </summary>
        /// <param name="shelfId">Optional shelf identifier used to filter results.</param>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Number of items per page. Default is <c>10</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a collection of <see cref="ShelfLevelReadDto"/>.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves shelf levels (v1)",
            Description = "Supports optional shelf filtering and pagination.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ShelfLevelReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ShelfLevelReadDto>>> GetShelfLevels(
            [FromQuery] int? shelfId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var list = await _svc.GetAllAsync(shelfId, page, pageSize, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific shelf level by its identifier.
        /// </summary>
        /// <param name="id">The shelf-level identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the requested <see cref="ShelfLevelReadDto"/>.</description></item>
        /// <item><description><c>404 NotFound</c> if the shelf level does not exist.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a shelf level by ID (v1)",
            Description = "Returns a single shelf level by its identifier.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(typeof(ShelfLevelReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ShelfLevelReadDto>> GetShelfLevel(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _svc.GetByIdAsync(id, cancellationToken);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new shelf level.
        /// Accessible only to Librarians and Admins.
        /// </summary>
        /// <param name="dto">The shelf-level DTO to create.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>201 Created</c> with the created <see cref="ShelfLevelReadDto"/> and its URI.</description></item>
        /// <item><description><c>400 BadRequest</c> if the payload is invalid.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user is not Librarian or Admin.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new shelf level (v1)",
            Description = "Accessible only to Librarians and Admins.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(typeof(ShelfLevelReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ShelfLevelReadDto>> CreateShelfLevel(
            [FromBody] ShelfLevelCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _svc.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetShelfLevel),
                new { id = created.ShelfLevelId },
                created);
        }

        /// <summary>
        /// Updates an existing shelf level.
        /// Accessible only to Librarians and Admins.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to update.</param>
        /// <param name="dto">The modified shelf-level DTO.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> when the update succeeds.</description></item>
        /// <item><description><c>400 BadRequest</c> if the route ID and body ID do not match.</description></item>
        /// <item><description><c>404 NotFound</c> if the shelf level does not exist.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user is not Librarian or Admin.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates a shelf level (v1)",
            Description = "Accessible only to Librarians and Admins.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateShelfLevel(
            [FromRoute] int id,
            [FromBody] ShelfLevelUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ShelfLevelId)
                return BadRequest(new { error = "IdMismatch", details = "Route ID and body ID do not match." });

            var ok = await _svc.UpdateAsync(dto, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a shelf level.
        /// Accessible only to Librarians and Admins.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> when deletion succeeds.</description></item>
        /// <item><description><c>404 NotFound</c> if the shelf level does not exist.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user is not Librarian or Admin.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a shelf level (v1)",
            Description = "Accessible only to Librarians and Admins.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteShelfLevel(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var ok = await _svc.DeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
    }
}
