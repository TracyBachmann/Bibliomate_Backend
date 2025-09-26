using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing authors.
    /// Provides CRUD and utility endpoints for author resources.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorsController"/> class.
        /// </summary>
        /// <param name="service">The service used to handle business logic for authors.</param>
        public AuthorsController(IAuthorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a list of authors.</returns>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieve all authors (v1)",
            Description = "Returns the complete list of authors stored in the system.",
            Tags = [ "Authors" ]
        )]
        [ProducesResponseType(typeof(IEnumerable<AuthorReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorReadDto>>> GetAuthors(
            CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a single author by its identifier.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the author data,  
        /// <c>404 NotFound</c> if the author does not exist.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieve author by ID (v1)",
            Description = "Fetches a single author resource using its unique identifier.",
            Tags = [ "Authors" ]
        )]
        [ProducesResponseType(typeof(AuthorReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthor(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var (dto, error) = await _service.GetByIdAsync(id, cancellationToken);
            if (error != null)
                return error;

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new author.
        /// </summary>
        /// <param name="dto">The data required to create the author.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with a location header pointing to the new author,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if the caller lacks permissions.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Create a new author (v1)",
            Description = "Creates and persists a new author resource.",
            Tags = ["Authors"]
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAuthor(
            [FromBody] AuthorCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (_, result) = await _service.CreateAsync(dto, cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an existing author.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <param name="dto">The updated author data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success,  
        /// <c>404 NotFound</c> if the author does not exist,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Update an author (v1)",
            Description = "Updates the specified author with new data.",
            Tags = [ "Authors" ]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateAuthor(
            [FromRoute] int id,
            [FromBody] AuthorCreateDto dto,
            CancellationToken cancellationToken)
        {
            if (!await _service.UpdateAsync(id, dto, cancellationToken))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes an author.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success,  
        /// <c>404 NotFound</c> if the author does not exist,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Delete an author (v1)",
            Description = "Removes an author resource by its unique identifier.",
            Tags = [ "Authors" ]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAuthor(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            if (!await _service.DeleteAsync(id, cancellationToken))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Searches authors by a query string.
        /// </summary>
        /// <param name="search">Optional search term to filter authors by name.</param>
        /// <param name="take">Maximum number of results to return. Defaults to 20.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a filtered list of authors.</returns>
        [HttpGet("search"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Search authors (v1)",
            Description = "Returns a subset of authors matching the provided search query.",
            Tags = [ "Authors" ]
        )]
        [ProducesResponseType(typeof(IEnumerable<AuthorReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorReadDto>>> SearchAuthors(
            [FromQuery] string? search,
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.SearchAsync(search, take, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Ensures an author exists by name.
        /// </summary>
        /// <param name="dto">The data required to check or create the author.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the existing author if found,  
        /// <c>201 Created</c> with the new author if it was created.
        /// </returns>
        [HttpPost("ensure"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Ensure author exists (v1)",
            Description = "Returns an existing author by name or creates it if missing.",
            Tags = [ "Authors" ]
        )]
        [ProducesResponseType(typeof(AuthorReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AuthorReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthorReadDto>> EnsureAuthor(
            [FromBody] AuthorCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (read, created) = await _service.EnsureAsync(dto.Name, cancellationToken);
            return created
                ? CreatedAtAction(nameof(GetAuthor), new { id = read.AuthorId }, read)
                : Ok(read);
        }
    }
}
