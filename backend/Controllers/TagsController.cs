using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Enums;

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
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            return await _context.Tags.ToListAsync();
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
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            return tag;
        }

        // POST: api/Tags
        /// <summary>
        /// Creates a new tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="tag">The tag entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created tag and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<Tag>> CreateTag(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, tag);
        }

        // PUT: api/Tags/{id}
        /// <summary>
        /// Updates an existing tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the tag to update.</param>
        /// <param name="tag">The modified tag entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the tag does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, Tag tag)
        {
            if (id != tag.TagId)
                return BadRequest();

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tags.Any(e => e.TagId == id))
                    return NotFound();
                throw;
            }

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
