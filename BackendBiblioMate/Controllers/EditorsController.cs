using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing editors (publishers).
    /// Provides CRUD endpoints for <see cref="EditorReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class EditorsController : ControllerBase
    {
        private readonly IEditorService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="EditorsController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating editor logic.</param>
        public EditorsController(IEditorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with list of <see cref="EditorReadDto"/>.
        /// </returns>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all editors (v1)",
            Description = "Returns the list of all editors.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(typeof(IEnumerable<EditorReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EditorReadDto>>> GetEditors(
            CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves an editor by its identifier.
        /// </summary>
        /// <param name="id">Editor identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="EditorReadDto"/>,  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves an editor by ID (v1)",
            Description = "Returns the editor with the specified ID.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(typeof(EditorReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEditor(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var (dto, error) = await _service.GetByIdAsync(id, cancellationToken);
            if (error != null)
                return error;
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new editor.
        /// </summary>
        /// <param name="dto">Data to create editor.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with location header,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new editor (v1)",
            Description = "Creates a new editor entry. Requires Admin or Librarian role.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(typeof(EditorReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEditor(
            [FromBody] EditorCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (createdDto, result) = await _service.CreateAsync(dto, cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an existing editor.
        /// </summary>
        /// <param name="id">Editor identifier.</param>
        /// <param name="dto">New editor data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing editor (v1)",
            Description = "Updates editor details by ID. Requires Admin or Librarian role.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateEditor(
            [FromRoute] int id,
            [FromBody] EditorUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!await _service.UpdateAsync(id, dto, cancellationToken))
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes an editor.
        /// </summary>
        /// <param name="id">Editor identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes an editor (v1)",
            Description = "Deletes the editor with the specified ID. Requires Admin or Librarian role.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteEditor(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            if (!await _service.DeleteAsync(id, cancellationToken))
                return NotFound();
            return NoContent();
        }
    }
}