using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing authors.
    /// Provides CRUD operations on <see cref="Author"/> entities.
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

        // GET: api/Authors
        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        /// <returns>A list of authors.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AuthorReadDto>>> GetAuthors()
        {
            var authors = await _context.Authors
                .Select(a => new AuthorReadDto
                {
                    AuthorId = a.AuthorId,
                    Name     = a.Name
                })
                .ToListAsync();

            return Ok(authors);
        }

        // GET: api/Authors/{id}
        /// <summary>
        /// Retrieves an author by ID.
        /// </summary>
        /// <param name="id">ID of the author to retrieve.</param>
        /// <returns>The requested author if found; otherwise <c>404 NotFound</c>.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthorReadDto>> GetAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            var dto = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name     = author.Name
            };

            return Ok(dto);
        }

        // POST: api/Authors
        /// <summary>
        /// Creates a new author.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="dto">Author to create.</param>
        /// <returns>The created author with its URI in the <c>Location</c> header.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<AuthorReadDto>> CreateAuthor(AuthorCreateDto dto)
        {
            var author = new Author
            {
                Name = dto.Name
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var resultDto = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name     = author.Name
            };

            return CreatedAtAction(nameof(GetAuthor), new { id = author.AuthorId }, resultDto);
        }

        // PUT: api/Authors/{id}
        /// <summary>
        /// Updates an existing author.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the author to update.</param>
        /// <param name="dto">Updated author data.</param>
        /// <returns><c>NoContent</c> on success; otherwise <c>400 BadRequest</c> if IDs do not match.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorCreateDto dto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            author.Name = dto.Name;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Authors/{id}
        /// <summary>
        /// Deletes an author.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the author to delete.</param>
        /// <returns><c>NoContent</c> on successful deletion; otherwise <c>404 NotFound</c>.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
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
