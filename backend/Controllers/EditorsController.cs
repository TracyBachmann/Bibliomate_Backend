using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

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
        private readonly BiblioMateDbContext _context;

        public EditorsController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Editors
        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        /// <returns>A collection of editors.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EditorReadDto>>> GetEditors()
        {
            var editors = await _context.Editors.ToListAsync();
            return Ok(editors.Select(e => new EditorReadDto
            {
                EditorId = e.EditorId,
                Name     = e.Name
            }));
        }

        // GET: api/Editors/{id}
        /// <summary>
        /// Retrieves an editor by its identifier.
        /// </summary>
        /// <param name="id">ID of the editor to retrieve.</param>
        /// <returns>
        /// The requested editor if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EditorReadDto>> GetEditor(int id)
        {
            var editor = await _context.Editors.FindAsync(id);
            if (editor == null)
                return NotFound();

            return Ok(new EditorReadDto
            {
                EditorId = editor.EditorId,
                Name     = editor.Name
            });
        }

        // POST: api/Editors
        /// <summary>
        /// Creates a new editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="dto">Editor to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and its URI;  
        /// <c>400 BadRequest</c> if validation fails.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<EditorReadDto>> CreateEditor(EditorCreateDto dto)
        {
            var editor = new Editor { Name = dto.Name };
            _context.Editors.Add(editor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEditor),
                new { id = editor.EditorId },
                new EditorReadDto
                {
                    EditorId = editor.EditorId,
                    Name     = editor.Name
                });
        }

        // PUT: api/Editors/{id}
        /// <summary>
        /// Updates an existing editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the editor to update.</param>
        /// <param name="dto">Updated editor data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEditor(int id, EditorCreateDto dto)
        {
            var editor = await _context.Editors.FindAsync(id);
            if (editor == null)
                return NotFound();

            editor.Name = dto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Editors/{id}
        /// <summary>
        /// Deletes an editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the editor to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the editor is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEditor(int id)
        {
            var editor = await _context.Editors.FindAsync(id);
            if (editor == null)
                return NotFound();

            _context.Editors.Remove(editor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}