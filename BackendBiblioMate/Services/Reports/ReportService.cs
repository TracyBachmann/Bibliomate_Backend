using System.Text;
using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Services.Reports
{
    /// <summary>
    /// Service that provides CRUD operations and analytical report generation.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="ReportService"/>.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        public ReportService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves all reports, ordered by generation date descending.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>List of <see cref="ReportReadDto"/>.</returns>
        public async Task<List<ReportReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var reports = await _context.Reports
                .AsNoTracking()
                .Include(r => r.User)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync(cancellationToken);

            return reports.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves all reports for a given user, ordered by generation date descending.
        /// </summary>
        /// <param name="userId">Identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>List of <see cref="ReportReadDto"/>.</returns>
        public async Task<List<ReportReadDto>> GetAllForUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync(cancellationToken);

            return reports.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves a single report by its identifier.
        /// </summary>
        /// <param name="reportId">Identifier of the report.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>
        /// The <see cref="ReportReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<ReportReadDto?> GetByIdAsync(
            int reportId,
            CancellationToken cancellationToken = default)
        {
            var report = await _context.Reports
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);

            return report == null ? null : ToDto(report);
        }

        /// <summary>
        /// Generates a new monthly report, saves it, and returns its DTO.
        /// </summary>
        /// <param name="dto">Report creation parameters (Title).</param>
        /// <param name="userId">Identifier of the user creating the report.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>The created <see cref="ReportReadDto"/>.</returns>
        public async Task<ReportReadDto> CreateAsync(
            ReportCreateDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var totalThisMonth = await _context.Loans
                .AsNoTracking()
                .CountAsync(l => l.LoanDate >= thisMonthStart, cancellationToken);

            var totalLastMonth = await _context.Loans
                .AsNoTracking()
                .CountAsync(l => l.LoanDate >= lastMonthStart && l.LoanDate < thisMonthStart, cancellationToken);

            var pctChange = totalLastMonth == 0
                ? 100
                : (int)Math.Round((totalThisMonth - (double)totalLastMonth) / totalLastMonth * 100);

            var topBooks = await _context.Loans
                .AsNoTracking()
                .Where(l => l.LoanDate >= thisMonthStart)
                .GroupBy(l => l.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(3)
                .Join(_context.Books.AsNoTracking(),
                      g => g.BookId,
                      b => b.BookId,
                      (g, b) => new { b.Title, g.Count })
                .ToListAsync(cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine($"Loans this month: {totalThisMonth}");
            sb.AppendLine($"Loans last month: {totalLastMonth} ({pctChange:+0;-0}% change)");
            sb.AppendLine("Top 3 books this month:");
            foreach (var tb in topBooks)
                sb.AppendLine($" - {tb.Title} ({tb.Count} loans)");

            var report = new Report
            {
                Title         = dto.Title,
                Content       = sb.ToString().TrimEnd(),
                UserId        = userId,
                GeneratedDate = now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(report)
                          .Reference(r => r.User)
                          .LoadAsync(cancellationToken);

            return ToDto(report);
        }

        /// <summary>
        /// Updates the title and content of an existing report.
        /// </summary>
        /// <param name="dto">Report update parameters.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>
        /// <c>true</c> if updated; <c>false</c> if not found.
        /// </returns>
        public async Task<bool> UpdateAsync(
            ReportUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var report = await _context.Reports
                .FindAsync(new object[] { dto.ReportId }, cancellationToken);

            if (report == null)
                return false;

            report.Title   = dto.Title;
            report.Content = dto.Content;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes a report by its identifier.
        /// </summary>
        /// <param name="reportId">Identifier of the report to delete.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>
        /// <c>true</c> if deleted; <c>false</c> if not found.
        /// </returns>
        public async Task<bool> DeleteAsync(
            int reportId,
            CancellationToken cancellationToken = default)
        {
            var report = await _context.Reports
                .FindAsync(new object[] { reportId }, cancellationToken);

            if (report == null)
                return false;

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Maps a <see cref="Report"/> entity to its DTO.
        /// </summary>
        private static ReportReadDto ToDto(Report r) => new()
        {
            ReportId      = r.ReportId,
            UserId        = r.UserId,
            UserName      = r.User != null ? $"{r.User.FirstName} {r.User.LastName}".Trim() : string.Empty,
            Title         = r.Title,
            Content       = r.Content,
            GeneratedDate = r.GeneratedDate
        };
    }
}