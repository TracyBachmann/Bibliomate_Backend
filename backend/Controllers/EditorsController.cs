using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing editors (publishers).
    /// Provides CRUD operations on <see cref="Editor"/> entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EditorsController : ControllerBase
    {
        private readonly IEditorService _service;

        /// <summary>
        /// Constructs the controller with its required service.
        /// </summary>
        /// <param name="service">Service for editor operations.</param>
        public EditorsController(IEditorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="EditorReadDto"/>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EditorReadDto>>> GetEditors()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Retrieves an editor by its identifier.
        /// </summary>
        /// <param name="id">ID of the editor to retrieve.</param>
        /// <returns>
        /// The requested <see cref="EditorReadDto"/> if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<EditorReadDto>> GetEditor(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="dto">Data for the new editor.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="EditorReadDto"/> and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<EditorReadDto>> CreateEditor(EditorCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetEditor),
                                   new { id = created.EditorId },
                                   created);
        }

        /// <summary>
        /// Updates an existing editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the editor to update.</param>
        /// <param name="dto">Updated editor data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success; <c>404 NotFound</c> if editor not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEditor(int id, EditorCreateDto dto)
        {
            if (!await _service.UpdateAsync(id, dto))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes an editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the editor to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success; <c>404 NotFound</c> if editor not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEditor(int id)
        {
            if (!await _service.DeleteAsync(id))
                return NotFound();

            return NoContent();
        }
    }
}