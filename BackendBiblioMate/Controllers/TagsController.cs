using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing tags.
    /// Provides CRUD endpoints that support catalog-wide tagging.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="TagReadDto"/>.
        /// </returns>
        [HttpGet, AllowAnonymous]
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
        /// <param name="id">The tag identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the <see cref="TagReadDto"/> if found;  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
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
        /// <param name="dto">The tag data to create.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="TagReadDto"/> and its URI;  
        /// <c>400 BadRequest</c> if validation fails;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
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
        /// <param name="id">The identifier of the tag to update.</param>
        /// <param name="dto">The updated tag data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if the tag does not exist;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
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
        /// <param name="id">The identifier of the tag to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if the tag is not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
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