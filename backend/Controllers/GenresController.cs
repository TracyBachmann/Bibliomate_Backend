using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
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
        /// <returns>A collection of <see cref="GenreReadDto"/>.</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreReadDto>>> GetGenres()
        {
            var genres = await _context.Genres.ToListAsync();
            return Ok(genres.Select(g => new GenreReadDto
            {
                GenreId = g.GenreId,
                Name = g.Name
            }));
        }

        // GET: api/Genres/{id}
        /// <summary>
        /// Retrieves a specific genre by its identifier.
        /// </summary>
        /// <param name="id">The genre identifier.</param>
        /// <returns>The requested <see cref="GenreReadDto"/> or <c>404 NotFound</c>.</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<GenreReadDto>> GetGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            return Ok(new GenreReadDto
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            });
        }

        // POST: api/Genres
        /// <summary>
        /// Creates a new genre.
        /// </summary>
        /// <param name="dto">The genre data to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created genre;  
        /// <c>400 BadRequest</c> if validation fails.
        /// </returns>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<GenreReadDto>> CreateGenre(GenreCreateDto dto)
        {
            var genre = new Genre { Name = dto.Name };
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenre), new { id = genre.GenreId },
                new GenreReadDto { GenreId = genre.GenreId, Name = genre.Name });
        }

        // PUT: api/Genres/{id}
        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="id">The ID of the genre to update.</param>
        /// <param name="dto">The updated genre data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if the genre does not exist.
        /// </returns>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, GenreCreateDto dto)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            genre.Name = dto.Name;

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
        /// </summary>
        /// <param name="id">The ID of the genre to delete.</param>
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
