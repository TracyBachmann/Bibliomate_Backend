using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing genres.
    /// Provides CRUD operations on <see cref="Genre"/> entities.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public GenresController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Genres
        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        /// <returns>A collection of genres.</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        // GET: api/Genres/{id}
        /// <summary>
        /// Retrieves a specific genre by its identifier.
        /// </summary>
        /// <param name="id">The genre identifier.</param>
        /// <returns>
        /// The requested genre if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            return genre;
        }

        // POST: api/Genres
        /// <summary>
        /// Creates a new genre.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="genre">The genre object to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and its URI;  
        /// <c>400 BadRequest</c> if validation fails.
        /// </returns>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<Genre>> CreateGenre(Genre genre)
        {
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenre), new { id = genre.GenreId }, genre);
        }

        // PUT: api/Genres/{id}
        /// <summary>
        /// Updates an existing genre.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The identifier of the genre to update.</param>
        /// <param name="genre">The updated genre object.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the genre does not exist.
        /// </returns>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, Genre genre)
        {
            if (id != genre.GenreId)
                return BadRequest();

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Genres.Any(g => g.GenreId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Genres/{id}
        /// <summary>
        /// Deletes a genre by its identifier.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The identifier of the genre to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the genre is not found.
        /// </returns>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}