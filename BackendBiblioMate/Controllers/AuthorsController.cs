using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing authors.
    /// Provides CRUD endpoints for <see cref="AuthorReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorsController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating author logic.</param>
        public AuthorsController(IAuthorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with list of <see cref="AuthorReadDto"/>.
        /// </returns>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all authors (v1)",
            Description = "Returns the list of all authors.",
            Tags = ["Authors"]
        )]
        [ProducesResponseType(typeof(IEnumerable<AuthorReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorReadDto>>> GetAuthors(
            CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves an author by its identifier.
        /// </summary>
        /// <param name="id">Author identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="AuthorReadDto"/>,  
        /// <c>404 NotFound</c> if missing.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves an author by ID (v1)",
            Description = "Returns the author matching the specified ID.",
            Tags = ["Authors"]
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
        /// <param name="dto">Data to create author.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with location header,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new author (v1)",
            Description = "Creates a new author entry.",
            Tags = ["Authors"]
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAuthor(
            [FromBody] AuthorCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (createdDto, result) = await _service.CreateAsync(dto, cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an existing author.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <param name="dto">New author data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if missing;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing author (v1)",
            Description = "Updates an author by ID.",
            Tags = ["Authors"]
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
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if missing;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes an author (v1)",
            Description = "Deletes an author by ID.",
            Tags = ["Authors"]
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
    }
}