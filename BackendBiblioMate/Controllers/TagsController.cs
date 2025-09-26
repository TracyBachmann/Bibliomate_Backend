using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing tags.
    /// Provides CRUD endpoints and utility operations
    /// that support catalog-wide tagging and autocomplete features.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="TagsController"/>.
        /// </summary>
        /// <param name="service">The tag service handling data access and business logic.</param>
        public TagsController(ITagService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <remarks>
        /// - Accessible anonymously.  
        /// - Returns the full list of tags.  
        /// </remarks>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a list of <see cref="TagReadDto"/>.</returns>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves all tags (v1)", Tags = ["Tags"])]
        [ProducesResponseType(typeof(IEnumerable<TagReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TagReadDto>>> GetTags(CancellationToken cancellationToken = default)
        {
            var list = await _service.GetAllAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific tag by its identifier.
        /// </summary>
        /// <remarks>
        /// - Accessible anonymously.  
        /// - Returns <c>404 Not Found</c> if the tag does not exist.  
        /// </remarks>
        /// <param name="id">The identifier of the tag.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the tag details.  
        /// <c>404 Not Found</c> if the tag does not exist.  
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves a tag by ID (v1)", Tags = ["Tags"])]
        [ProducesResponseType(typeof(TagReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagReadDto>> GetTag([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> and <c>Librarian</c> roles.  
        /// - On success, the <c>Location</c> header points to the created resource.  
        /// </remarks>
        /// <param name="dto">The tag creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>201 Created</c> with the created <see cref="TagReadDto"/>.</returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Creates a new tag (v1)", Tags = ["Tags"])]
        [ProducesResponseType(typeof(TagReadDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<TagReadDto>> CreateTag([FromBody] TagCreateDto dto, CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetTag), new { id = created.TagId }, created);
        }

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> and <c>Librarian</c> roles.  
        /// - The <paramref name="id"/> route parameter must match <see cref="TagUpdateDto.TagId"/>.  
        /// - Returns <c>404 Not Found</c> if the tag does not exist.  
        /// </remarks>
        /// <param name="id">The identifier of the tag to update.</param>
        /// <param name="dto">The updated tag data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if successfully updated.  
        /// <c>400 Bad Request</c> if IDs mismatch.  
        /// <c>404 Not Found</c> if the tag does not exist.  
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Updates a tag (v1)", Tags = ["Tags"])]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag([FromRoute] int id, [FromBody] TagUpdateDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.TagId) return BadRequest("Route ID and payload TagId do not match.");
            var ok = await _service.UpdateAsync(dto, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> and <c>Librarian</c> roles.  
        /// - Permanently removes the tag from the system.  
        /// </remarks>
        /// <param name="id">The identifier of the tag to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if successfully deleted.  
        /// <c>404 Not Found</c> if the tag does not exist.  
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Deletes a tag (v1)", Tags = ["Tags"])]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var ok = await _service.DeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Searches tags by name (for autocomplete).
        /// </summary>
        /// <remarks>
        /// - Accessible anonymously.  
        /// - Used primarily for UI autocomplete and quick lookups.  
        /// </remarks>
        /// <param name="search">Optional search term. If <c>null</c>, returns top tags.</param>
        /// <param name="take">Maximum number of results to return (1..100). Default: 20.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a filtered list of <see cref="TagReadDto"/>.</returns>
        [HttpGet("search"), AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<TagReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TagReadDto>>> Search(
            [FromQuery] string? search,
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.SearchAsync(search, take, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Ensures a tag exists: creates it if it does not already exist.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Librarian</c> and <c>Admin</c> roles.  
        /// - Idempotent: returns existing tag if found, otherwise creates a new one.  
        /// </remarks>
        /// <param name="dto">The tag creation data (only the name is required).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the new <see cref="TagReadDto"/> if created.  
        /// <c>200 OK</c> with the existing <see cref="TagReadDto"/> if already present.  
        /// </returns>
        [HttpPost("ensure"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(typeof(TagReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(TagReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TagReadDto>> Ensure(
            [FromBody] TagCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (read, created) = await _service.EnsureAsync(dto.Name, cancellationToken);
            return created
                ? CreatedAtAction(nameof(GetTag), new { id = read.TagId }, read)
                : Ok(read);
        }
    }
}