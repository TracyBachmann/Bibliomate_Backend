using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Tests.Services
{
    public class ReportServiceTest
    {
        private readonly ReportService _service;
        private readonly BiblioMateDbContext _db;

        public ReportServiceTest()
        {
            // In-memory EF + minimal config for EncryptionService (if needed elsewhere)
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new BiblioMateDbContext(options, encryptionService);

            _service = new ReportService(_db);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllReports()
        {
            // seed users and reports
            _db.Users.Add(new User { UserId = 1, Name = "User1" });
            _db.Users.Add(new User { UserId = 2, Name = "User2" });
            _db.Reports.Add(new Report
            {
                Title = "R1",
                // Content will be overwritten by logic but that's fine
                Content = "placeholder",
                UserId = 1,
                GeneratedDate = DateTime.UtcNow
            });
            _db.Reports.Add(new Report
            {
                Title = "R2",
                Content = "placeholder",
                UserId = 2,
                GeneratedDate = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var list = (await _service.GetAllAsync()).ToList();

            Assert.Equal(2, list.Count);
            Assert.Contains(list, r => r.Title == "R1");
            Assert.Contains(list, r => r.Title == "R2");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnReport_WhenExists()
        {
            _db.Users.Add(new User { UserId = 3, Name = "User3" });
            var report = new Report
            {
                Title = "Test",
                Content = "placeholder",
                UserId = 3,
                GeneratedDate = DateTime.UtcNow
            };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(report.ReportId);

            Assert.NotNull(dto);
            Assert.Equal("Test", dto!.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var dto = await _service.GetByIdAsync(999);
            Assert.Null(dto);
        }

        [Fact]
        public async Task CreateAsync_ShouldGenerateAndSaveReport()
        {
            // Seed user and some loans to test calculations
            var userId = 10;
            _db.Users.Add(new User { UserId = userId, Name = "User10" });

            // Add two loans this month, one last month
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            _db.Loans.Add(new Loan { LoanDate = thisMonthStart.AddDays(1), BookId = 1 });
            _db.Loans.Add(new Loan { LoanDate = thisMonthStart.AddDays(2), BookId = 2 });
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            _db.Loans.Add(new Loan { LoanDate = lastMonthStart.AddDays(1), BookId = 1 });

            // Add books for title lookup
            _db.Books.Add(new Book { BookId = 1, Title = "Book A" });
            _db.Books.Add(new Book { BookId = 2, Title = "Book B" });

            await _db.SaveChangesAsync();

            var createDto = new ReportCreateDto { Title = "Monthly Stats" };
            var created = await _service.CreateAsync(createDto, userId);

            Assert.NotNull(created);
            Assert.Equal("Monthly Stats", created.Title);
            Assert.False(string.IsNullOrWhiteSpace(created.Content));
            Assert.Contains("Loans this month: 2", created.Content);
            Assert.Contains("Loans last month: 1", created.Content);
            Assert.Contains("Book A", created.Content);
            Assert.Contains("Book B", created.Content);

            // Confirm it was persisted
            Assert.True(await _db.Reports.AnyAsync(r => r.ReportId == created.ReportId));
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyReport_WhenExists()
        {
            _db.Users.Add(new User { UserId = 4, Name = "User4" });
            var report = new Report
            {
                Title = "Old",
                Content = "OldC",
                UserId = 4,
                GeneratedDate = DateTime.UtcNow
            };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            var updateDto = new ReportUpdateDto
            {
                ReportId = report.ReportId,
                Title = "Updated",
                Content = "UpdatedC"
            };
            var success = await _service.UpdateAsync(updateDto);

            Assert.True(success);
            var updated = await _db.Reports.FindAsync(report.ReportId);
            Assert.Equal("Updated", updated!.Title);
            Assert.Equal("UpdatedC", updated.Content);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var updateDto = new ReportUpdateDto
            {
                ReportId = 999,
                Title = "X",
                Content = "Y"
            };
            var success = await _service.UpdateAsync(updateDto);
            Assert.False(success);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveReport_WhenExists()
        {
            _db.Users.Add(new User { UserId = 5, Name = "User5" });
            var report = new Report
            {
                Title = "Del",
                Content = "DelC",
                UserId = 5,
                GeneratedDate = DateTime.UtcNow
            };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(report.ReportId);

            Assert.True(success);
            Assert.False(await _db.Reports.AnyAsync(r => r.ReportId == report.ReportId));
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var success = await _service.DeleteAsync(999);
            Assert.False(success);
        }
    }
}