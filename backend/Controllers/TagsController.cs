using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Enums;
using backend.DTOs;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing tags.
    /// Provides CRUD endpoints that support catalog-wide tagging.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public TagsController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <returns>A collection of <see cref="Tag"/>.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TagReadDto>>> GetTags()
        {
            var tags = await _context.Tags.ToListAsync();

            var dtos = tags.Select(tag => new TagReadDto
            {
                TagId = tag.TagId,
                Name = tag.Name
            });

            return Ok(dtos);
        }

        // GET: api/Tags/{id}
        /// <summary>
        /// Retrieves a specific tag by its identifier.
        /// </summary>
        /// <param name="id">The tag identifier.</param>
        /// <returns>
        /// The requested tag if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TagReadDto>> GetTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            return new TagReadDto
            {
                TagId = tag.TagId,
                Name = tag.Name
            };
        }

        // POST: api/Tags
        /// <summary>
        /// Creates a new tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The tag entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created tag and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<TagReadDto>> CreateTag(TagCreateDto dto)
        {
            var tag = new Tag
            {
                Name = dto.Name
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            var result = new TagReadDto
            {
                TagId = tag.TagId,
                Name = tag.Name
            };

            return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, result);
        }

        // PUT: api/Tags/{id}
        /// <summary>
        /// Updates an existing tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the tag to update.</param>
        /// <param name="dto">The modified tag entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the tag does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, TagUpdateDto dto)
        {
            if (id != dto.TagId)
                return BadRequest();

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            tag.Name = dto.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Tags/{id}
        /// <summary>
        /// Deletes a tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the tag to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the tag is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
