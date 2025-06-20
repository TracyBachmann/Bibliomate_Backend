using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers
{
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
        /// Retrieves all reports, ordered by generation date (newest first).
        /// Only accessible by Admin or Librarian.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            return await _context.Reports
                .Include(r => r.User)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();
        }

        // GET: api/Reports/5
        /// <summary>
        /// Retrieves a specific report by its ID.
        /// Only the owner or Admin can access it.
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (report.UserId != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            return report;
        }

        // POST: api/Reports
        /// <summary>
        /// Creates a new report for the current user.
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Report>> CreateReport(Report report)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            report.UserId = currentUserId;
            report.GeneratedDate = DateTime.UtcNow;

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), new { id = report.ReportId }, report);
        }

        // PUT: api/Reports/5
        /// <summary>
        /// Updates an existing report. Only the author or an admin can perform this action.
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, Report report)
        {
            if (id != report.ReportId)
                return BadRequest();

            var existing = await _context.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.ReportId == id);
            if (existing == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (existing.UserId != currentUserId && !User.IsInRole("Admin"))
                return Forbid("Only the author or an admin can modify this report.");

            report.GeneratedDate = existing.GeneratedDate;
            report.UserId = existing.UserId;

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reports.Any(r => r.ReportId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Reports/5
        /// <summary>
        /// Deletes a specific report.
        /// Only the author or an admin can perform this action.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (report.UserId != currentUserId && !User.IsInRole("Admin"))
                return Forbid("Only the author or an admin can delete this report.");

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
