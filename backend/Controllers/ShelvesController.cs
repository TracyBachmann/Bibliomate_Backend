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
        /// <returns>A paginated collection of <see cref="ShelfReadDto"/>.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfReadDto>>> GetShelves(
            int? zoneId,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Shelves
                                .Include(s => s.Zone)
                                .Include(s => s.Genre)
                                .AsQueryable();

            if (zoneId.HasValue)
                query = query.Where(s => s.ZoneId == zoneId.Value);

            var shelves = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var shelfDtos = shelves.Select(s => new ShelfReadDto
            {
                ShelfId = s.ShelfId,
                Name = s.Name,
                ZoneId = s.ZoneId,
                ZoneName = s.Zone?.Name ?? "Unknown",
                GenreId = s.GenreId,
                GenreName = s.Genre?.Name ?? "Unknown",
                Capacity = s.Capacity,
                CurrentLoad = s.CurrentLoad
            });

            return Ok(shelfDtos);
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
        public async Task<ActionResult<ShelfReadDto>> GetShelf(int id)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Zone)
                .Include(s => s.Genre)
                .FirstOrDefaultAsync(s => s.ShelfId == id);

            if (shelf == null)
                return NotFound();

            var dto = new ShelfReadDto
            {
                ShelfId = shelf.ShelfId,
                Name = shelf.Name,
                ZoneId = shelf.ZoneId,
                ZoneName = shelf.Zone?.Name ?? "Unknown",
                GenreId = shelf.GenreId,
                GenreName = shelf.Genre?.Name ?? "Unknown",
                Capacity = shelf.Capacity,
                CurrentLoad = shelf.CurrentLoad
            };

            return Ok(dto);
        }

        // POST: api/Shelves
        /// <summary>
        /// Creates a new shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The shelf entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created shelf and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<ShelfReadDto>> CreateShelf(ShelfCreateDto dto)
        {
            var shelf = new Shelf
            {
                Name = dto.Name,
                ZoneId = dto.ZoneId,
                GenreId = dto.GenreId,
                Capacity = dto.Capacity,
                CurrentLoad = 0
            };

            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();

            await _context.Entry(shelf).Reference(s => s.Zone).LoadAsync();
            await _context.Entry(shelf).Reference(s => s.Genre).LoadAsync();

            var createdDto = new ShelfReadDto
            {
                ShelfId = shelf.ShelfId,
                Name = shelf.Name,
                ZoneId = shelf.ZoneId,
                ZoneName = shelf.Zone?.Name ?? "Unknown",
                GenreId = shelf.GenreId,
                GenreName = shelf.Genre?.Name ?? "Unknown",
                Capacity = shelf.Capacity,
                CurrentLoad = shelf.CurrentLoad
            };

            return CreatedAtAction(nameof(GetShelf), new { id = shelf.ShelfId }, createdDto);
        }

        // PUT: api/Shelves/{id}
        /// <summary>
        /// Updates an existing shelf.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the shelf to update.</param>
        /// <param name="dto">The modified shelf entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the shelf does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelf(int id, ShelfUpdateDto dto)
        {
            if (id != dto.ShelfId)
                return BadRequest();

            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf == null)
                return NotFound();

            shelf.Name = dto.Name;
            shelf.ZoneId = dto.ZoneId;
            shelf.GenreId = dto.GenreId;
            shelf.Capacity = dto.Capacity;

            await _context.SaveChangesAsync();

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