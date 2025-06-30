using backend.DTOs;
using backend.Helpers;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing analytical reports.
    /// Users can create and access their own reports;
    /// Librarians and Admins have broader access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="ReportsController"/>.
        /// </summary>
        /// <param name="service">The report service.</param>
        public ReportsController(IReportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all reports (Admins & Librarians only).
        /// </summary>
        /// <returns>A collection of <see cref="ReportReadDto"/>.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportReadDto>>> GetReports()
            => Ok(await _service.GetAllAsync());

        /// <summary>
        /// Retrieves a specific report.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        /// <remarks>User must be author or Admin.</remarks>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportReadDto>> GetReport(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();

            var currentUser = TokenHelper.GetUserId(User);
            if (dto.UserId != currentUser && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new report for the current user.
        /// </summary>
        /// <param name="dto">The report creation data.</param>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ReportReadDto>> CreateReport(ReportCreateDto dto)
        {
            var userId = TokenHelper.GetUserId(User);
            var created = await _service.CreateAsync(dto, userId);
            return CreatedAtAction(
                nameof(GetReport),
                new { id = created.ReportId },
                created
            );
        }

        /// <summary>
        /// Updates an existing report.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        /// <param name="dto">The updated report data.</param>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, ReportUpdateDto dto)
        {
            if (id != dto.ReportId) return BadRequest();

            // fetch existing to check ownership
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUser = TokenHelper.GetUserId(User);
            if (existing.UserId != currentUser && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var ok = await _service.UpdateAsync(dto);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a specific report.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUser = TokenHelper.GetUserId(User);
            if (existing.UserId != currentUser && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
