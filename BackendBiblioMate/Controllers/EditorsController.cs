using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing editors (publishers).
    /// Provides CRUD operations and utility endpoints for editor resources.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class EditorsController : ControllerBase
    {
        private readonly IEditorService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorsController"/> class.
        /// </summary>
        /// <param name="service">The service used to encapsulate business logic for editors.</param>
        public EditorsController(IEditorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a list of editors.</returns>
        [HttpGet, AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all editors (v1)",
            Description = "Returns the complete list of editors.",
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
        /// Retrieves a single editor by its unique identifier.
        /// </summary>
        /// <param name="id">The editor identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the editor,  
        /// <c>404 Not Found</c> if the editor does not exist.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves an editor by ID (v1)",
            Description = "Fetches a single editor by its unique identifier.",
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
        /// Creates a new editor. Requires Admin or Librarian role.
        /// </summary>
        /// <param name="dto">The data required to create the editor.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created editor,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new editor (v1)",
            Description = "Creates and persists a new editor entry. Requires Admin or Librarian role.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(typeof(EditorReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEditor(
            [FromBody] EditorCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (_, result) = await _service.CreateAsync(dto, cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an existing editor. Requires Admin or Librarian role.
        /// </summary>
        /// <param name="id">The editor identifier.</param>
        /// <param name="dto">The updated editor data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if update was successful,  
        /// <c>404 Not Found</c> if the editor does not exist,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing editor (v1)",
            Description = "Updates an editor by its unique identifier. Requires Admin or Librarian role.",
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
        /// Deletes an editor by its unique identifier. Requires Admin or Librarian role.
        /// </summary>
        /// <param name="id">The editor identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if deletion was successful,  
        /// <c>404 Not Found</c> if the editor does not exist,  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access is denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes an editor (v1)",
            Description = "Deletes an editor by its unique identifier. Requires Admin or Librarian role.",
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
        
        /// <summary>
        /// Searches editors by a query string.
        /// </summary>
        /// <param name="search">Optional search term to filter editors by name.</param>
        /// <param name="take">Maximum number of results to return. Defaults to 20.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a filtered list of editors.</returns>
        [HttpGet("search"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Search editors (v1)",
            Description = "Returns a list of editors matching the provided query string.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(typeof(IEnumerable<EditorReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EditorReadDto>>> SearchEditors(
            [FromQuery] string? search,
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.SearchAsync(search, take, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Ensures an editor exists by name. If not, creates a new one. Requires Admin or Librarian role.
        /// </summary>
        /// <param name="dto">The data used to check or create the editor.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the existing editor,  
        /// <c>201 Created</c> with the new editor if it was created.
        /// </returns>
        [HttpPost("ensure"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Ensure editor exists (v1)",
            Description = "Checks if an editor exists by name, or creates it if missing.",
            Tags = ["Editors"]
        )]
        [ProducesResponseType(typeof(EditorReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(EditorReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<EditorReadDto>> EnsureEditor(
            [FromBody] EditorCreateDto dto,
            CancellationToken cancellationToken)
        {
            var (read, created) = await _service.EnsureAsync(dto.Name, cancellationToken);
            return created
                ? CreatedAtAction(nameof(GetEditor), new { id = read.EditorId }, read)
                : Ok(read);
        }
    }
}
