using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing shelf levels.
    /// Provides CRUD operations and paginated queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ShelfLevelsController : ControllerBase
    {
        private readonly IShelfLevelService _svc;

        /// <summary>
        /// Constructs a new <see cref="ShelfLevelsController"/>.
        /// </summary>
        /// <param name="svc">Service for shelf level operations.</param>
        public ShelfLevelsController(IShelfLevelService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// Retrieves all shelf levels with optional shelf filtering and pagination.
        /// </summary>
        /// <param name="shelfId">Optional shelf identifier used to filter results.</param>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Number of items per page. Default is <c>10</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="ShelfLevelReadDto"/>.
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
        /// <c>200 OK</c> with the requested <see cref="ShelfLevelReadDto"/>,
        /// or <c>404 NotFound</c> if it does not exist.
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
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The shelf-level DTO to create.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and its URI.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new shelf level (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(typeof(ShelfLevelReadDto), StatusCodes.Status201Created)]
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
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to update.</param>
        /// <param name="dto">The modified shelf-level DTO.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if the IDs do not match;
        /// <c>404 NotFound</c> if the shelf level does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates a shelf level (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateShelfLevel(
            [FromRoute] int id,
            [FromBody] ShelfLevelUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ShelfLevelId) return BadRequest();
            var ok = await _svc.UpdateAsync(dto, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a shelf level.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;
        /// <c>404 NotFound</c> if the shelf level is not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a shelf level (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["ShelfLevels"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteShelfLevel(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var ok = await _svc.DeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
    }
}