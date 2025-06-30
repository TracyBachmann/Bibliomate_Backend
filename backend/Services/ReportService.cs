using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Provides CRUD operations and analytical report generation.
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

            return report == null 
                ? null 
                : ToDto(report);
        }

        /// <inheritdoc/>
        public async Task<ReportReadDto> CreateAsync(ReportCreateDto dto, int userId)
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // 1) Count total loans this and last month
            var totalThisMonth = await _context.Loans
                .CountAsync(l => l.LoanDate >= thisMonthStart);
            var totalLastMonth = await _context.Loans
                .CountAsync(l => l.LoanDate >= lastMonthStart
                              && l.LoanDate < thisMonthStart);

            // 2) Compute percentage change
            var pctChange = totalLastMonth == 0
                ? 100
                : (int)Math.Round((totalThisMonth - (double)totalLastMonth)
                                  / totalLastMonth * 100);

            // 3) Determine top 3 most-loaned books this month
            var topBooks = await _context.Loans
                .Where(l => l.LoanDate >= thisMonthStart)
                .GroupBy(l => l.BookId)
                .Select(g => new 
                {
                    BookId = g.Key,
                    Count  = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .Join(_context.Books,
                      g => g.BookId,
                      b => b.BookId,
                      (g, b) => new { b.Title, g.Count })
                .ToListAsync();

            // 4) Build report content
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Loans this month: {totalThisMonth}");
            sb.AppendLine($"Loans last month: {totalLastMonth} ({pctChange:+0;-0}% change)");
            sb.AppendLine("Top 3 books this month:");
            foreach (var tb in topBooks)
            {
                sb.AppendLine($" - {tb.Title} ({tb.Count} loans)");
            }

            // 5) Persist the report
            var report = new Report
            {
                Title         = dto.Title,
                Content       = sb.ToString().TrimEnd(),
                UserId        = userId,
                GeneratedDate = now
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Load User navigation property for DTO
            await _context.Entry(report)
                          .Reference(r => r.User)
                          .LoadAsync();

            // 6) Return the fully populated dto
            return new ReportReadDto
            {
                ReportId      = report.ReportId,
                UserId        = report.UserId,
                UserName      = report.User.Name,
                Title         = report.Title,
                Content       = report.Content,
                GeneratedDate = report.GeneratedDate
            };
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(ReportUpdateDto dto)
        {
            var existing = await _context.Reports.FindAsync(dto.ReportId);
            if (existing == null) 
                return false;

            existing.Title   = dto.Title;
            existing.Content = dto.Content;
            // leave GeneratedDate and UserId unchanged

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) 
                return false;

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
