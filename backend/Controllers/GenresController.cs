using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        // GET: api/Genres/5
        /// <summary>
        /// Retrieves a specific genre by ID.
        /// </summary>
        /// <param name="id">The ID of the genre.</param>
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
        /// </summary>
        /// <param name="genre">The genre object to create.</param>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<Genre>> CreateGenre(Genre genre)
        {
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenre), new { id = genre.GenreId }, genre);
        }

        // PUT: api/Genres/5
        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="id">The ID of the genre to update.</param>
        /// <param name="genre">The updated genre object.</param>
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
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Genres/5
        /// <summary>
        /// Deletes a genre by ID.
        /// </summary>
        /// <param name="id">The ID of the genre to delete.</param>
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