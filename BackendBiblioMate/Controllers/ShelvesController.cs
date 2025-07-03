using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing shelves.
    /// Provides CRUD and paginated, zone-filtered endpoints for <see cref="ShelfReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
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
        /// <param name="zoneId">Optional zone identifier to filter results.</param>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>10</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with list of <see cref="ShelfReadDto"/>.
        /// </returns>
        [HttpGet, Authorize]
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
        /// <param name="id">The shelf identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="ShelfReadDto"/>,  
        /// <c>404 NotFound</c> if missing.
        /// </returns>
        [HttpGet("{id}"), Authorize]
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
        /// <param name="dto">Data to create the shelf.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with created <see cref="ShelfReadDto"/> and location header,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
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
        /// <param name="id">The identifier of the shelf to update.</param>
        /// <param name="dto">New shelf data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if missing;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
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
        /// <param name="id">The identifier of the shelf to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if missing;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
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