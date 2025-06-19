using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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
        /// Retrieves all shelves, with optional filtering by zone and pagination.
        /// </summary>
        /// <param name="zoneId">Optional zone ID to filter shelves.</param>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="pageSize">Items per page (default is 10).</param>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shelf>>> GetShelves(int? zoneId, int page = 1, int pageSize = 10)
        {
            var query = _context.Shelves.Include(s => s.Zone).AsQueryable();

            if (zoneId.HasValue)
                query = query.Where(s => s.ZoneId == zoneId.Value);

            var shelves = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(shelves);
        }

        // GET: api/Shelves/5
        /// <summary>
        /// Retrieves a specific shelf by ID, including its zone. Requires authentication.
        /// </summary>
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
        /// Creates a new shelf. Only Admins and Librarians are authorized.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<Shelf>> CreateShelf(Shelf shelf)
        {
            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelf), new { id = shelf.ShelfId }, shelf);
        }

        // PUT: api/Shelves/5
        /// <summary>
        /// Updates an existing shelf. Only Admins and Librarians are authorized.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Shelves/5
        /// <summary>
        /// Deletes a shelf by ID. Only Admins and Librarians are authorized.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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