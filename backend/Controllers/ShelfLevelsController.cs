using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing shelf levels.
    /// Provides CRUD operations and paginated queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ShelfLevelsController : ControllerBase
    {
        private readonly IShelfLevelService _svc;

        public ShelfLevelsController(IShelfLevelService svc)
        {
            _svc = svc;
        }

        // GET: api/ShelfLevels
        /// <summary>
        /// Retrieves all shelf levels with optional shelf filtering and pagination.
        /// </summary>
        /// <param name="shelfId">
        /// Optional shelf identifier used to filter results.
        /// </param>
        /// <param name="page">
        /// Page index (1-based). Default is <c>1</c>.
        /// </param>
        /// <param name="pageSize">
        /// Number of items per page. Default is <c>10</c>.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="ShelfLevelReadDto"/>.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfLevelReadDto>>> GetShelfLevels(
            int? shelfId,
            int page = 1,
            int pageSize = 10)
        {
            var list = await _svc.GetAllAsync(shelfId, page, pageSize);
            return Ok(list);
        }

        // GET: api/ShelfLevels/{id}
        /// <summary>
        /// Retrieves a specific shelf level by its identifier.
        /// </summary>
        /// <param name="id">The shelf-level identifier.</param>
        /// <returns>
        /// <c>200 OK</c> with the requested <see cref="ShelfLevelReadDto"/>,
        /// or <c>404 NotFound</c> if it does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfLevelReadDto>> GetShelfLevel(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/ShelfLevels
        /// <summary>
        /// Creates a new shelf level.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The shelf-level DTO to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<ShelfLevelReadDto>> CreateShelfLevel(ShelfLevelCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetShelfLevel),
                new { id = created.ShelfLevelId },
                created
            );
        }

        // PUT: api/ShelfLevels/{id}
        /// <summary>
        /// Updates an existing shelf level.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to update.</param>
        /// <param name="dto">The modified shelf-level DTO.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if the IDs do not match;
        /// <c>404 NotFound</c> if the shelf level does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelfLevel(int id, ShelfLevelUpdateDto dto)
        {
            if (id != dto.ShelfLevelId) return BadRequest();
            var ok = await _svc.UpdateAsync(dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: api/ShelfLevels/{id}
        /// <summary>
        /// Deletes a shelf level.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;
        /// <c>404 NotFound</c> if the shelf level is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelfLevel(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
