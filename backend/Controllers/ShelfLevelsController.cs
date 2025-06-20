using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
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
        /// A paginated collection of <see cref="ShelfLevel"/>;  
        /// always <c>200 OK</c>.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfLevel>>> GetShelfLevels(
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

            return Ok(shelfLevels);
        }

        // GET: api/ShelfLevels/{id}
        /// <summary>
        /// Retrieves a specific shelf level by its identifier.
        /// </summary>
        /// <param name="id">The shelf-level identifier.</param>
        /// <returns>
        /// The requested <see cref="ShelfLevel"/> with its shelf details  
        /// or <c>404 NotFound</c> if it does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfLevel>> GetShelfLevel(int id)
        {
            var shelfLevel = await _context.ShelfLevels
                .Include(sl => sl.Shelf)
                .FirstOrDefaultAsync(sl => sl.ShelfLevelId == id);

            if (shelfLevel == null)
                return NotFound();

            return shelfLevel;
        }

        // POST: api/ShelfLevels
        /// <summary>
        /// Creates a new shelf level.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="shelfLevel">The shelf-level entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<ShelfLevel>> CreateShelfLevel(
            ShelfLevel shelfLevel)
        {
            _context.ShelfLevels.Add(shelfLevel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelfLevel),
                new { id = shelfLevel.ShelfLevelId }, shelfLevel);
        }

        // PUT: api/ShelfLevels/{id}
        /// <summary>
        /// Updates an existing shelf level.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf level to update.</param>
        /// <param name="shelfLevel">The modified shelf-level entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the shelf level does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelfLevel(
            int id,
            ShelfLevel shelfLevel)
        {
            if (id != shelfLevel.ShelfLevelId)
                return BadRequest();

            _context.Entry(shelfLevel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ShelfLevels.Any(sl => sl.ShelfLevelId == id))
                    return NotFound();
                throw;
            }

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
