using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
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
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            return await _context.Tags.ToListAsync();
        }

        // GET: api/Tags/5
        /// <summary>
        /// Retrieves a specific tag by ID.
        /// </summary>
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
        /// Creates a new tag. Admins and librarians only.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<Tag>> CreateTag(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, tag);
        }

        // PUT: api/Tags/5
        /// <summary>
        /// Updates an existing tag. Admins and librarians only.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Tags/5
        /// <summary>
        /// Deletes a tag by ID. Admins and librarians only.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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