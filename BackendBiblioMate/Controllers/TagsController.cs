using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing tags.
    /// Provides CRUD endpoints that support catalog-wide tagging.
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
        /// <param name="service">Service encapsulating tag logic.</param>
        public TagsController(ITagService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all tags (v1)",
            Description = "Returns all tags in the system.",
            Tags = ["Tags"]
        )]
        [ProducesResponseType(typeof(IEnumerable<TagReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TagReadDto>>> GetTags(
            CancellationToken cancellationToken = default)
        {
            var list = await _service.GetAllAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific tag by its identifier.
        /// </summary>
        [HttpGet("{id}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a tag by ID (v1)",
            Description = "Returns a single tag by its identifier.",
            Tags = ["Tags"]
        )]
        [ProducesResponseType(typeof(TagReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagReadDto>> GetTag(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto is null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new tag (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Tags"]
        )]
        [ProducesResponseType(typeof(TagReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<TagReadDto>> CreateTag(
            [FromBody] TagCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetTag), new { id = created.TagId }, created);
        }

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates a tag (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Tags"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateTag(
            [FromRoute] int id,
            [FromBody] TagUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.TagId)
                return BadRequest("Route ID and payload TagId do not match.");

            var ok = await _service.UpdateAsync(dto, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a tag (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Tags"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTag(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var ok = await _service.DeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
    }
}