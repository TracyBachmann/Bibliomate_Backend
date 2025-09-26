using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing genres.
    /// Provides CRUD operations and utility endpoints for genres.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenresController"/> class.
        /// </summary>
        /// <param name="service">The service encapsulating genre-related business logic.</param>
        public GenresController(IGenreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a list of genres.</returns>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all genres (v1)",
            Description = "Returns the complete list of genres.",
            Tags = ["Genres"]
        )]
        [ProducesResponseType(typeof(IEnumerable<GenreReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GenreReadDto>>> GetGenres(
            CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a single genre by its unique identifier.
        /// </summary>
        /// <param name="id">The genre identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the genre,  
        /// <c>404 Not Found</c> if the genre does not exist.
        /// </returns>
        [HttpGet("{id:int}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a genre by ID (v1)",
            Description = "Fetches a single genre by its unique identifier.",
            Tags = ["Genres"]
        )]
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
        /// Creates a new genre. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="dto">The data required to create the genre.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created genre,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new genre (v1)",
            Description = "Creates and persists a new genre entry. Requires Librarian or Admin role.",
            Tags = ["Genres"]
        )]
        [ProducesResponseType(typeof(GenreReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateGenre(
            [FromBody] GenreCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (_, result) = await _service.CreateAsync(dto, cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an existing genre. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="id">The genre identifier.</param>
        /// <param name="dto">The updated genre data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if update was successful,  
        /// <c>404 Not Found</c> if the genre does not exist,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpPut("{id:int}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing genre (v1)",
            Description = "Updates an existing genre by ID. Requires Librarian or Admin role.",
            Tags = ["Genres"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateGenre(
            [FromRoute] int id,
            [FromBody] GenreUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!await _service.UpdateAsync(id, dto, cancellationToken))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a genre by its unique identifier. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="id">The genre identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if deletion was successful,  
        /// <c>404 Not Found</c> if the genre does not exist,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpDelete("{id:int}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a genre (v1)",
            Description = "Deletes a genre by its unique identifier. Requires Librarian or Admin role.",
            Tags = ["Genres"]
        )]
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

        /// <summary>
        /// Searches genres by a query string. Supports autocomplete scenarios.
        /// </summary>
        /// <param name="search">Optional search term to filter genres by name.</param>
        /// <param name="take">Maximum number of results to return. Defaults to 20.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with the filtered list of genres.</returns>
        [HttpGet("search"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Search genres (v1)",
            Description = "Returns a list of genres matching the provided query string.",
            Tags = ["Genres"]
        )]
        [ProducesResponseType(typeof(IEnumerable<GenreReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GenreReadDto>>> Search(
            [FromQuery] string? search,
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.SearchAsync(search, take, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Ensures a genre exists by name. If not, creates a new one. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="dto">The data used to check or create the genre.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the existing genre,  
        /// <c>201 Created</c> with the new genre if it was created.
        /// </returns>
        [HttpPost("ensure"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Ensure genre exists (v1)",
            Description = "Checks if a genre exists by name, or creates it if missing.",
            Tags = ["Genres"]
        )]
        [ProducesResponseType(typeof(GenreReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GenreReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GenreReadDto>> Ensure(
            [FromBody] GenreCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (read, created) = await _service.EnsureAsync(dto.Name, cancellationToken);
            return created
                ? CreatedAtAction(nameof(GetGenre), new { id = read.GenreId }, read)
                : Ok(read);
        }
    }
}

