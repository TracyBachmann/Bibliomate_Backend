using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IZoneService _svc;

        public ZonesController(IZoneService svc)
        {
            _svc = svc;
        }

        // GET: api/Zones
        /// <summary>
        /// Retrieves all zones with optional pagination.
        /// </summary>
        /// <param name="page">Page index (1-based). Default = 1.</param>
        /// <param name="pageSize">Items per page. Default = 10.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="ZoneReadDto"/>.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ZoneReadDto>>> GetZones(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var dtos = await _svc.GetAllAsync(page, pageSize);
            return Ok(dtos);
        }

        // GET: api/Zones/{id}
        /// <summary>
        /// Retrieves a specific zone by its identifier.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="ZoneReadDto"/> if found;
        /// <c>404 NotFound</c> otherwise.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ZoneReadDto>> GetZone(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/Zones
        /// <summary>
        /// Creates a new zone.
        /// </summary>
        /// <param name="dto">Zone data to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="ZoneReadDto"/>.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<ZoneReadDto>> CreateZone(ZoneCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetZone), new { id = created.ZoneId }, created);
        }

        // PUT: api/Zones/{id}
        /// <summary>
        /// Updates an existing zone.
        /// </summary>
        /// <param name="id">The identifier of the zone to update.</param>
        /// <param name="dto">Updated zone data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if zone not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZone(int id, ZoneUpdateDto dto)
        {
            if (id != dto.ZoneId) return BadRequest();
            var ok = await _svc.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/Zones/{id}
        /// <summary>
        /// Deletes a zone.
        /// </summary>
        /// <param name="id">The identifier of the zone to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if zone not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
