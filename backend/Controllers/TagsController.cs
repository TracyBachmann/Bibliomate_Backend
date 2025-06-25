using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing tags.
    /// Provides CRUD endpoints that support catalog-wide tagging.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _svc;

        public TagsController(ITagService svc)
        {
            _svc = svc;
        }

        // GET: api/Tags
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="TagReadDto"/>.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TagReadDto>>> GetTags()
        {
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }

        // GET: api/Tags/{id}
        /// <summary>
        /// Retrieves a specific tag by its identifier.
        /// </summary>
        /// <param name="id">The tag identifier.</param>
        /// <returns>
        /// <c>200 OK</c> with the <see cref="TagReadDto"/> if found;
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TagReadDto>> GetTag(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/Tags
        /// <summary>
        /// Creates a new tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The tag data to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="TagReadDto"/> and its URI;
        /// <c>400 BadRequest</c> if validation fails.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<TagReadDto>> CreateTag(TagCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetTag), new { id = created.TagId }, created);
        }

        // PUT: api/Tags/{id}
        /// <summary>
        /// Updates an existing tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the tag to update.</param>
        /// <param name="dto">The updated tag data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if IDs mismatch;
        /// <c>404 NotFound</c> if the tag does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, TagUpdateDto dto)
        {
            if (id != dto.TagId) return BadRequest();
            var ok = await _svc.UpdateAsync(dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: api/Tags/{id}
        /// <summary>
        /// Deletes a tag.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the tag to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if the tag is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}