using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing authors.
    /// Allows CRUD operations on author entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public AuthorsController(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            return await _context.Authors.ToListAsync();
        }

        /// <summary>
        /// Retrieves an author by ID.
        /// </summary>
        /// <param name="id">ID of the author to retrieve.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Author>> GetAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            return author;
        }

        /// <summary>
        /// Creates a new author.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="author">Author to create.</param>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<Author>> CreateAuthor(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAuthor), new { id = author.AuthorId }, author);
        }

        /// <summary>
        /// Updates an existing author.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the author to update.</param>
        /// <param name="author">Updated author data.</param>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, Author author)
        {
            if (id != author.AuthorId)
                return BadRequest();

            _context.Entry(author).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an author.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the author to delete.</param>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}