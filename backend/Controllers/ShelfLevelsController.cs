using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

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
        private readonly BiblioMateDbContext _context;

        public ShelfLevelsController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/ShelfLevels
        /// <summary>
        /// Retrieves all shelf levels with optional shelf filtering and pagination.
        /// </summary>
        /// <param name="shelfId">Optional shelf identifier used to filter results.</param>
        /// <param name="page">Page index (1-based). Default is 1.</param>
        /// <param name="pageSize">Number of items per page. Default is 10.</param>
        /// <returns>
        /// A paginated collection of <see cref="ShelfLevelReadDto"/>;  
        /// always <c>200 OK</c>.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfLevelReadDto>>> GetShelfLevels(
            int? shelfId,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.ShelfLevels
                                .Include(sl => sl.Shelf)
                                .AsQueryable();

            if (shelfId.HasValue)
                query = query.Where(sl => sl.ShelfId == shelfId.Value);

            var shelfLevels = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(shelfLevels.Select(sl => new ShelfLevelReadDto
            {
                ShelfLevelId = sl.ShelfLevelId,
                LevelNumber = sl.LevelNumber,
                ShelfId = sl.ShelfId,
                ShelfName = sl.Shelf.Name
            }));
        }

        // GET: api/ShelfLevels/{id}
        /// <summary>
        /// Retrieves a specific shelf level by its identifier.
        /// </summary>
        /// <param name="id">The shelf-level identifier.</param>
        /// <returns>
        /// The requested <see cref="ShelfLevelReadDto"/> with its shelf details  
        /// or <c>404 NotFound</c> if it does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfLevelReadDto>> GetShelfLevel(int id)
        {
            var shelfLevel = await _context.ShelfLevels
                .Include(sl => sl.Shelf)
                .FirstOrDefaultAsync(sl => sl.ShelfLevelId == id);

            if (shelfLevel == null)
                return NotFound();

            return Ok(new ShelfLevelReadDto
            {
                ShelfLevelId = shelfLevel.ShelfLevelId,
                LevelNumber = shelfLevel.LevelNumber,
                ShelfId = shelfLevel.ShelfId,
                ShelfName = shelfLevel.Shelf.Name
            });
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
            var shelfLevel = new ShelfLevel
            {
                LevelNumber = dto.LevelNumber,
                ShelfId = dto.ShelfId
            };

            _context.ShelfLevels.Add(shelfLevel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelfLevel),
                new { id = shelfLevel.ShelfLevelId },
                new ShelfLevelReadDto
                {
                    ShelfLevelId = shelfLevel.ShelfLevelId,
                    LevelNumber = shelfLevel.LevelNumber,
                    ShelfId = shelfLevel.ShelfId,
                    ShelfName = (await _context.Shelves.FindAsync(shelfLevel.ShelfId))?.Name ?? "Unknown"
                });
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
            if (id != dto.ShelfLevelId)
                return BadRequest();

            var existing = await _context.ShelfLevels.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.LevelNumber = dto.LevelNumber;
            existing.ShelfId = dto.ShelfId;

            await _context.SaveChangesAsync();

            return NoContent();
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
            var shelfLevel = await _context.ShelfLevels.FindAsync(id);
            if (shelfLevel == null)
                return NotFound();

            _context.ShelfLevels.Remove(shelfLevel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
