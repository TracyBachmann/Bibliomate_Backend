using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Helpers;
using backend.Models.Enums;

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
        private readonly BiblioMateDbContext _context;

        public ReportsController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports
        /// <summary>
        /// Retrieves all reports ordered by generation date (newest first).
        /// </summary>
        /// <remarks>Accessible to Librarians and Admins only.</remarks>
        /// <returns>A collection of reports with related user information.</returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            return await _context.Reports
                .Include(r => r.User)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();
        }

        // GET: api/Reports/{id}
        /// <summary>
        /// Retrieves a specific report by its identifier.
        /// </summary>
        /// <param name="id">The report identifier.</param>
        /// <returns>
        /// The requested report if authorized;  
        /// otherwise <c>403 Forbid</c> or <c>404 NotFound</c>.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
                return NotFound();

            var currentUserId = TokenHelper.GetUserId(User);
            if (report.UserId != currentUserId && !User.IsInRole(UserRoles.Admin))
                return Forbid();

            return report;
        }

        // POST: api/Reports
        /// <summary>
        /// Creates a new report for the current user.
        /// </summary>
        /// <param name="report">The report entity to create (content only).</param>
        /// <returns>
        /// <c>201 Created</c> with the created report and its URI.
        /// </returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Report>> CreateReport(Report report)
        {
            var currentUserId  = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            report.UserId      = currentUserId;
            report.GeneratedDate = DateTime.UtcNow;

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), new { id = report.ReportId }, report);
        }

        // PUT: api/Reports/{id}
        /// <summary>
        /// Updates an existing report.
        /// Allowed to the report’s author or an Admin.
        /// </summary>
        /// <param name="id">The identifier of the report to update.</param>
        /// <param name="report">The modified report entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>403 Forbid</c> if the user lacks permission;  
        /// <c>404 NotFound</c> if the report does not exist.
        /// </returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, Report report)
        {
            if (id != report.ReportId)
                return BadRequest();

            var existing = await _context.Reports.AsNoTracking()
                                .FirstOrDefaultAsync(r => r.ReportId == id);
            if (existing == null)
                return NotFound();

            var currentUserId = TokenHelper.GetUserId(User);
            if (existing.UserId != currentUserId && !User.IsInRole(UserRoles.Admin))
                return Forbid("Only the author or an admin can modify this report.");

            report.GeneratedDate = existing.GeneratedDate;
            report.UserId        = existing.UserId;

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reports.Any(r => r.ReportId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Reports/{id}
        /// <summary>
        /// Deletes a specific report.
        /// Allowed to the report’s author or an Admin.
        /// </summary>
        /// <param name="id">The identifier of the report to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>403 Forbid</c> if unauthorized;  
        /// <c>404 NotFound</c> if the report is not found.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound();

            var currentUserId = TokenHelper.GetUserId(User);
            if (report.UserId != currentUserId && !User.IsInRole(UserRoles.Admin))
                return Forbid("Only the author or an admin can delete this report.");

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
