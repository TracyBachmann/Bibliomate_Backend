using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

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
        private readonly BiblioMateDbContext _context;

        public ShelvesController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Shelves
        /// <summary>
        /// Retrieves all shelves with optional zone filtering and pagination.
        /// </summary>
        /// <param name="zoneId">Optional zone identifier to filter results.</param>
        /// <param name="page">Page index (1-based). Default is 1.</param>
        /// <param name="pageSize">Items per page. Default is 10.</param>
        /// <returns>A paginated collection of <see cref="Shelf"/>.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shelf>>> GetShelves(
            int? zoneId,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Shelves
                                .Include(s => s.Zone)
                                .AsQueryable();

            if (zoneId.HasValue)
                query = query.Where(s => s.ZoneId == zoneId.Value);

            var shelves = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(shelves);
        }

        // GET: api/Shelves/{id}
        /// <summary>
        /// Retrieves a specific shelf by its identifier.
        /// </summary>
        /// <param name="id">The shelf identifier.</param>
        /// <returns>
        /// The requested shelf with its zone details  
        /// or <c>404 NotFound</c> if it does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Shelf>> GetShelf(int id)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Zone)
                .FirstOrDefaultAsync(s => s.ShelfId == id);

            if (shelf == null)
                return NotFound();

            return shelf;
        }

        // POST: api/Shelves
        /// <summary>
        /// Creates a new shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="shelf">The shelf entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created shelf and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<Shelf>> CreateShelf(Shelf shelf)
        {
            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelf), new { id = shelf.ShelfId }, shelf);
        }

        // PUT: api/Shelves/{id}
        /// <summary>
        /// Updates an existing shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf to update.</param>
        /// <param name="shelf">The modified shelf entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the shelf does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelf(int id, Shelf shelf)
        {
            if (id != shelf.ShelfId)
                return BadRequest();

            _context.Entry(shelf).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Shelves.Any(s => s.ShelfId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Shelves/{id}
        /// <summary>
        /// Deletes a shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the shelf is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf == null)
                return NotFound();

            _context.Shelves.Remove(shelf);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
