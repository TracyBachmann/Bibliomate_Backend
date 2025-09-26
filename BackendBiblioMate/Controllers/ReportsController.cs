using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing analytical reports.
    /// Provides CRUD endpoints for <see cref="ReportReadDto"/> entities
    /// and enforces user ownership rules.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsController"/> class.
        /// </summary>
        /// <param name="service">Service for report operations.</param>
        public ReportsController(IReportService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Retrieves all reports.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="ReportReadDto"/>.</description></item>
        /// <item><description><c>403 Forbidden</c> if the caller is not an Admin or Librarian.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all reports (v1)",
            Description = "Accessible only by Admins and Librarians.",
            Tags = ["Reports"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ReportReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ReportReadDto>>> GetReports(
            CancellationToken cancellationToken = default)
        {
            var reports = await _service.GetAllAsync(cancellationToken);
            return Ok(reports);
        }

        /// <summary>
        /// Retrieves a specific report by its identifier.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with <see cref="ReportReadDto"/>.</description></item>
        /// <item><description><c>404 NotFound</c> if the report does not exist.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user is not the owner and not an Admin.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a report by ID (v1)",
            Description = "Enforces ownership or Admin access.",
            Tags = ["Reports"]
        )]
        [ProducesResponseType(typeof(ReportReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReportReadDto>> GetReport(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var currentUser = User.GetUserId();
            if (dto.UserId != currentUser && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new report for the current user.
        /// </summary>
        /// <param name="dto">The report creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>201 Created</c> with the created <see cref="ReportReadDto"/>.</description></item>
        /// <item><description><c>400 BadRequest</c> if the payload is invalid.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new report (v1)",
            Description = "Creates a new report for the current user.",
            Tags = ["Reports"]
        )]
        [ProducesResponseType(typeof(ReportReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReportReadDto>> CreateReport(
            [FromBody] ReportCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            var created = await _service.CreateAsync(dto, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetReport),
                new { id = created.ReportId },
                created);
        }

        /// <summary>
        /// Updates an existing report.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        /// <param name="dto">The updated report data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> on success.</description></item>
        /// <item><description><c>400 BadRequest</c> if IDs mismatch.</description></item>
        /// <item><description><c>404 NotFound</c> if the report does not exist.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user is not the owner and not an Admin.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing report (v1)",
            Description = "Updates a report if the user owns it or is Admin.",
            Tags = ["Reports"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateReport(
            [FromRoute] int id,
            [FromBody] ReportUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.ReportId)
                return BadRequest();

            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound();

            var currentUser = User.GetUserId();
            if (existing.UserId != currentUser && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var updated = await _service.UpdateAsync(dto, cancellationToken);
            return updated ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a specific report.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>204 NoContent</c> on success.</description></item>
        /// <item><description><c>404 NotFound</c> if the report does not exist.</description></item>
        /// <item><description><c>403 Forbidden</c> if the user is not the owner and not an Admin.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request is not authenticated.</description></item>
        /// </list>
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a report (v1)",
            Description = "Deletes a report if the user owns it or is Admin.",
            Tags = ["Reports"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReport(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound();

            var currentUser = User.GetUserId();
            if (existing.UserId != currentUser && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var deleted = await _service.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
    }
}
