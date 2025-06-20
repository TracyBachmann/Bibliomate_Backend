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
        public async Task<ActionResult<IEnumerable<ZoneReadDto>>> GetZones(
            int page = 1,
            int pageSize = 10)
        {
            var zones = await _context.Zones
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = zones.Select(z => new ZoneReadDto
            {
                ZoneId     = z.ZoneId,
                FloorNumber = z.FloorNumber,
                AisleCode   = z.AisleCode,
                Description = z.Description
            });

            return Ok(result);
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
        public async Task<ActionResult<ZoneReadDto>> GetZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
                return NotFound();

            return new ZoneReadDto
            {
                ZoneId     = zone.ZoneId,
                FloorNumber = zone.FloorNumber,
                AisleCode   = zone.AisleCode,
                Description = zone.Description
            };
        }

        // POST: api/Zones
        /// <summary>
        /// Creates a new zone.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The zone entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created zone and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<ZoneReadDto>> CreateZone(ZoneCreateDto dto)
        {
            var zone = new Zone
            {
                FloorNumber = dto.FloorNumber,
                AisleCode   = dto.AisleCode,
                Description = dto.Description
            };

            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            var result = new ZoneReadDto
            {
                ZoneId     = zone.ZoneId,
                FloorNumber = zone.FloorNumber,
                AisleCode   = zone.AisleCode,
                Description = zone.Description
            };

            return CreatedAtAction(nameof(GetZone), new { id = zone.ZoneId }, result);
        }

        // PUT: api/Zones/{id}
        /// <summary>
        /// Updates an existing zone.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the zone to update.</param>
        /// <param name="dto">The modified zone entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the zone does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZone(int id, ZoneUpdateDto dto)
        {
            if (id != dto.ZoneId)
                return BadRequest();

            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
                return NotFound();

            zone.FloorNumber = dto.FloorNumber;
            zone.AisleCode   = dto.AisleCode;
            zone.Description = dto.Description;

            await _context.SaveChangesAsync();
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
