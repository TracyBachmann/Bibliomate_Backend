using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfLevel>>> GetShelfLevels()
        {
            return await _context.ShelfLevels
                .Include(sl => sl.Shelf)
                .ToListAsync();
        }

        // GET: api/ShelfLevels/5
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
        [HttpPost]
        public async Task<ActionResult<ShelfLevel>> CreateShelfLevel(ShelfLevel shelfLevel)
        {
            _context.ShelfLevels.Add(shelfLevel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShelfLevel), new { id = shelfLevel.ShelfLevelId }, shelfLevel);
        }

        // PUT: api/ShelfLevels/5
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
