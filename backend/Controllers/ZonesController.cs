using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

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
        /// <summary>
        /// Retrieves all zones with optional pagination. Requires authentication.
        /// </summary>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="pageSize">Items per page (default is 10).</param>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Zone>>> GetZones(int page = 1, int pageSize = 10)
        {
            var zones = await _context.Zones
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(zones);
        }

        // GET: api/Zones/5
        /// <summary>
        /// Retrieves a specific zone by ID. Requires authentication.
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Zone>> GetZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
                return NotFound();

            return zone;
        }

        // POST: api/Zones
        /// <summary>
        /// Creates a new zone. Only Librarians and Admins are authorized.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<Zone>> CreateZone(Zone zone)
        {
            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetZone), new { id = zone.ZoneId }, zone);
        }

        // PUT: api/Zones/5
        /// <summary>
        /// Updates an existing zone. Only Librarians and Admins are authorized.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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
        /// <summary>
        /// Deletes a zone by ID. Only Librarians and Admins are authorized.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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