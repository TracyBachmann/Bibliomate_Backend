using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZonesController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public ZonesController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Zones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Zone>>> GetZones()
        {
            return await _context.Zones.ToListAsync();
        }

        // GET: api/Zones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Zone>> GetZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
                return NotFound();

            return zone;
        }

        // POST: api/Zones
        [HttpPost]
        public async Task<ActionResult<Zone>> CreateZone(Zone zone)
        {
            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetZone), new { id = zone.ZoneId }, zone);
        }

        // PUT: api/Zones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZone(int id, Zone zone)
        {
            if (id != zone.ZoneId)
                return BadRequest();

            _context.Entry(zone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Zones.Any(z => z.ZoneId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Zones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
                return NotFound();

            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
