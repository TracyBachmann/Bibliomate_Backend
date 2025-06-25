using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Provides CRUD operations for analytical reports.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Constructs a new instance of <see cref="ReportService"/>.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        public ReportService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ReportReadDto>> GetAllAsync()
        {
            var reports = await _context.Reports
                .Include(r => r.User)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();

            return reports.Select(ToDto);
        }

        /// <inheritdoc/>
        public async Task<ReportReadDto?> GetByIdAsync(int reportId)
        {
            var report = await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            return report == null ? null : ToDto(report);
        }

        /// <inheritdoc/>
        public async Task<ReportReadDto> CreateAsync(ReportCreateDto dto, int userId)
        {
            var report = new Report
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = userId,
                GeneratedDate = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // load User navigation
            await _context.Entry(report).Reference(r => r.User).LoadAsync();

            return ToDto(report);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(ReportUpdateDto dto)
        {
            var existing = await _context.Reports.FindAsync(dto.ReportId);
            if (existing == null) return false;

            existing.Title = dto.Title;
            existing.Content = dto.Content;
            // keep GeneratedDate & UserId intact

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ReportReadDto ToDto(Report r) => new()
        {
            ReportId      = r.ReportId,
            UserId        = r.UserId,
            UserName      = r.User.Name,
            Title         = r.Title,
            Content       = r.Content,
            GeneratedDate = r.GeneratedDate
        };
    }
}
