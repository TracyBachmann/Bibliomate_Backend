using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing editors (publishers).
    /// Allows CRUD operations on editor entities.
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

        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Editor>>> GetEditors()
        {
            return await _context.Editors.ToListAsync();
        }

        /// <summary>
        /// Retrieves an editor by ID.
        /// </summary>
        /// <param name="id">ID of the editor to retrieve.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Editor>> GetEditor(int id)
        {
            var editor = await _context.Editors.FindAsync(id);
            if (editor == null)
                return NotFound();

            return editor;
        }

        /// <summary>
        /// Creates a new editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="editor">Editor to create.</param>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<Editor>> CreateEditor(Editor editor)
        {
            _context.Editors.Add(editor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEditor), new { id = editor.EditorId }, editor);
        }

        /// <summary>
        /// Updates an existing editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the editor to update.</param>
        /// <param name="editor">Updated editor data.</param>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEditor(int id, Editor editor)
        {
            if (id != editor.EditorId)
                return BadRequest();

            _context.Entry(editor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an editor.
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">ID of the editor to delete.</param>
        [Authorize(Roles = "Admin,Librarian")]
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