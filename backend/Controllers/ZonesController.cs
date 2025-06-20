using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing library zones (physical areas in which shelves are organized).
    /// Supports paginated queries and full CRUD operations.
    /// </summary>
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
        /// Retrieves all zones with optional pagination.
        /// </summary>
        /// <param name="page">Page index (1-based). Default is 1.</param>
        /// <param name="pageSize">Number of items per page. Default is 10.</param>
        /// <returns>A paginated collection of <see cref="Zone"/> objects.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Zone>>> GetZones(
            int page = 1,
            int pageSize = 10)
        {
            var zones = await _context.Zones
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(zones);
        }

        // GET: api/Zones/{id}
        /// <summary>
        /// Retrieves a specific zone by its identifier.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <returns>
        /// The requested zone if found; otherwise <c>404 NotFound</c>.
        /// </returns>
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
        /// Creates a new zone.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="zone">The zone entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created zone and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<Zone>> CreateZone(Zone zone)
        {
            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetZone), new { id = zone.ZoneId }, zone);
        }

        // PUT: api/Zones/{id}
        /// <summary>
        /// Updates an existing zone.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the zone to update.</param>
        /// <param name="zone">The modified zone entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the zone does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
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
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Zones/{id}
        /// <summary>
        /// Deletes a zone.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the zone to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the zone is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
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
