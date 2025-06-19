using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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
        /// Retrieves all shelf levels, with optional filtering by shelf and pagination.
        /// </summary>
        /// <param name="shelfId">Optional shelf ID to filter shelf levels.</param>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="pageSize">Items per page (default is 10).</param>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfLevel>>> GetShelfLevels(int? shelfId, int page = 1, int pageSize = 10)
        {
            var query = _context.ShelfLevels.Include(sl => sl.Shelf).AsQueryable();

            if (shelfId.HasValue)
                query = query.Where(sl => sl.ShelfId == shelfId.Value);

            var shelfLevels = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(shelfLevels);
        }

        // GET: api/ShelfLevels/5
        /// <summary>
        /// Retrieves a specific shelf level by ID, including its shelf. Requires authentication.
        /// </summary>
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
        /// Creates a new shelf level. Only Admins and Librarians can perform this action.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<ShelfLevel>> CreateShelfLevel(ShelfLevel shelfLevel)
        {
            _context.ShelfLevels.Add(shelfLevel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelfLevel), new { id = shelfLevel.ShelfLevelId }, shelfLevel);
        }

        // PUT: api/ShelfLevels/5
        /// <summary>
        /// Updates an existing shelf level. Only Admins and Librarians can perform this action.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelfLevel(int id, ShelfLevel shelfLevel)
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
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/ShelfLevels/5
        /// <summary>
        /// Deletes a shelf level by ID. Only Admins and Librarians can perform this action.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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
