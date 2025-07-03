using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing genres.
    /// Provides CRUD endpoints for <see cref="GenreReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="GenresController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating genre logic.</param>
        public GenresController(IGenreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with list of <see cref="GenreReadDto"/>.
        /// </returns>
        [HttpGet, AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<GenreReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GenreReadDto>>> GetGenres(
            CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a genre by its identifier.
        /// </summary>
        /// <param name="id">Genre identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="GenreReadDto"/>,  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        [ProducesResponseType(typeof(GenreReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGenre(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var (dto, error) = await _service.GetByIdAsync(id, cancellationToken);
            if (error != null)
                return error;
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new genre.
        /// </summary>
        /// <param name="dto">Data to create genre.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with location header,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(typeof(GenreReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateGenre(
            [FromBody] GenreCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (createdDto, result) = await _service.CreateAsync(dto, cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="id">Genre identifier.</param>
        /// <param name="dto">New genre data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateGenre(
            [FromRoute] int id,
            [FromBody] GenreUpdateDto dto,               // ← Utilisation de GenreUpdateDto
            CancellationToken cancellationToken)
        {
            if (!await _service.UpdateAsync(id, dto, cancellationToken))
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a genre.
        /// </summary>
        /// <param name="id">Genre identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteGenre(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            if (!await _service.DeleteAsync(id, cancellationToken))
                return NotFound();
            return NoContent();
        }
    }
}