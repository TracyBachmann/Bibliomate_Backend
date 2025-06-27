using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IGenreService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="GenresController"/>.
        /// </summary>
        /// <param name="service">Service for genre operations.</param>
        public GenresController(IGenreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="GenreReadDto"/>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreReadDto>>> GetGenres()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific genre by its identifier.
        /// </summary>
        /// <param name="id">The genre identifier.</param>
        /// <returns>
        /// The requested <see cref="GenreReadDto"/> if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<GenreReadDto>> GetGenre(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new genre.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="dto">The genre data to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="GenreReadDto"/> and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<GenreReadDto>> CreateGenre(GenreCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetGenre),
                new { id = created.GenreId },
                created);
        }

        /// <summary>
        /// Updates an existing genre.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The identifier of the genre to update.</param>
        /// <param name="dto">The updated genre data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success; <c>404 NotFound</c> if genre not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, GenreCreateDto dto)
        {
            if (!await _service.UpdateAsync(id, dto))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a genre by its identifier.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The identifier of the genre to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success; <c>404 NotFound</c> if genre not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            if (!await _service.DeleteAsync(id))
                return NotFound();

            return NoContent();
        }
    }
}