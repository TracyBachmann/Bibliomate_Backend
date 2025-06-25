using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing shelves.
    /// Supports CRUD operations and paginated, zone-filtered queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ShelvesController : ControllerBase
    {
        private readonly IShelfService _svc;

        public ShelvesController(IShelfService svc)
        {
            _svc = svc;
        }

        // GET: api/Shelves
        /// <summary>
        /// Retrieves all shelves with optional zone filtering and pagination.
        /// </summary>
        /// <param name="zoneId">Optional zone identifier to filter results.</param>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>10</c>.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="ShelfReadDto"/>.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfReadDto>>> GetShelves(
            int? zoneId,
            int page = 1,
            int pageSize = 10)
        {
            var list = await _svc.GetAllAsync(zoneId, page, pageSize);
            return Ok(list);
        }

        // GET: api/Shelves/{id}
        /// <summary>
        /// Retrieves a specific shelf by its identifier.
        /// </summary>
        /// <param name="id">The shelf identifier.</param>
        /// <returns>
        /// <c>200 OK</c> with the requested <see cref="ShelfReadDto"/>,
        /// or <c>404 NotFound</c> if it does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfReadDto>> GetShelf(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/Shelves
        /// <summary>
        /// Creates a new shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The shelf entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="ShelfReadDto"/> and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<ShelfReadDto>> CreateShelf(ShelfCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetShelf), new { id = created.ShelfId }, created);
        }

        // PUT: api/Shelves/{id}
        /// <summary>
        /// Updates an existing shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf to update.</param>
        /// <param name="dto">The modified shelf entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if the IDs do not match;
        /// <c>404 NotFound</c> if the shelf does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelf(int id, ShelfUpdateDto dto)
        {
            if (id != dto.ShelfId) return BadRequest();
            var ok = await _svc.UpdateAsync(dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: api/Shelves/{id}
        /// <summary>
        /// Deletes a shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if the shelf is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}