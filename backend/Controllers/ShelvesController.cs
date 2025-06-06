using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

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
                [HttpGet]
                public async Task<ActionResult<IEnumerable<Shelf>>> GetShelves()
                {
                    return await _context.Shelves
                        .Include(s => s.Zone)
                        .ToListAsync(); 
                }

        // GET: api/Shelves/5
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
        [HttpPost]
        public async Task<ActionResult<Shelf>> CreateShelf(Shelf shelf)
        {
            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelf), new { id = shelf.ShelfId }, shelf);
        }

        // PUT: api/Shelves/5
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
