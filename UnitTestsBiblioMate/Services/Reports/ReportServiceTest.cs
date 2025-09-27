using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Reports;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTestsBiblioMate.Services.Reports
{
    /// <summary>
    /// Unit tests for <see cref="ReportService"/>.
    /// Verifies CRUD operations and report generation logic.
    /// </summary>
    public class ReportServiceTest
    {
        private readonly ReportService       _service;
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes an EF Core InMemory database and a ReportService instance.
        /// Uses <see cref="EncryptionService"/> since DbContext requires it.
        /// </summary>
        public ReportServiceTest()
        {
            // Configure EncryptionService with a fixed 32-byte Base64 key
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            // Use InMemory EF Core for isolated unit testing
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db      = new BiblioMateDbContext(options, encryptionService);
            _service = new ReportService(_db);
        }

        /// <summary>
        /// Ensures GetAllAsync returns all reports from the database.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllReports()
        {
            // Arrange: seed users and reports
            _db.Users.Add(new User { UserId = 1, FirstName = "User1", LastName = "" });
            _db.Users.Add(new User { UserId = 2, FirstName = "User2", LastName = "" });
            _db.Reports.Add(new Report
            {
                Title         = "R1",
                Content       = "placeholder",
                UserId        = 1,
                GeneratedDate = DateTime.UtcNow
            });
            _db.Reports.Add(new Report
            {
                Title         = "R2",
                Content       = "placeholder",
                UserId        = 2,
                GeneratedDate = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // Act
            var list = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Contains(list, r => r.Title == "R1");
            Assert.Contains(list, r => r.Title == "R2");
        }

        /// <summary>
        /// Ensures GetByIdAsync returns a DTO when the report exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnReport_WhenExists()
        {
            // Arrange: seed a user and a report
            _db.Users.Add(new User { UserId = 3, FirstName = "User3", LastName = "" });
            var report = new Report
            {
                Title         = "Test",
                Content       = "placeholder",
                UserId        = 3,
                GeneratedDate = DateTime.UtcNow
            };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            // Act
            var dto = await _service.GetByIdAsync(report.ReportId);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("Test", dto.Title);
        }

        /// <summary>
        /// Ensures GetByIdAsync returns null when the report does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var dto = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(dto);
        }

        /// <summary>
        /// Ensures CreateAsync generates a report containing:
        /// - Loan statistics (this month vs last month).
        /// - Titles of the books involved.
        /// Also verifies persistence in the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldGenerateAndSaveReport()
        {
            // Arrange: seed a user and some loans across two months
            const int userId = 10;
            _db.Users.Add(new User { UserId = userId, FirstName = "User10", LastName = "" });

            var now            = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // Two loans in this month, one in the previous month
            _db.Loans.Add(new Loan { LoanDate = thisMonthStart.AddDays(1), BookId = 1 });
            _db.Loans.Add(new Loan { LoanDate = thisMonthStart.AddDays(2), BookId = 2 });
            _db.Loans.Add(new Loan { LoanDate = lastMonthStart.AddDays(1), BookId = 1 });

            // Books for inclusion in the report
            _db.Books.Add(new Book { BookId = 1, Title = "Book A" });
            _db.Books.Add(new Book { BookId = 2, Title = "Book B" });

            await _db.SaveChangesAsync();

            var createDto = new ReportCreateDto { Title = "Monthly Stats" };

            // Act
            var created = await _service.CreateAsync(createDto, userId);

            // Assert: verify mapping and generated content
            Assert.NotNull(created);
            Assert.Equal("Monthly Stats", created.Title);
            Assert.False(string.IsNullOrWhiteSpace(created.Content));
            Assert.Contains("Loans this month: 2", created.Content);
            Assert.Contains("Loans last month: 1", created.Content);
            Assert.Contains("Book A", created.Content);
            Assert.Contains("Book B", created.Content);

            // Confirm report was persisted
            Assert.True(await _db.Reports.AnyAsync(r => r.ReportId == created.ReportId));
        }

        /// <summary>
        /// Ensures UpdateAsync modifies an existing report
        /// and persists the changes.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyReport_WhenExists()
        {
            // Arrange
            _db.Users.Add(new User { UserId = 4, FirstName = "User4", LastName = "" });
            var report = new Report
            {
                Title         = "Old",
                Content       = "OldC",
                UserId        = 4,
                GeneratedDate = DateTime.UtcNow
            };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            var updateDto = new ReportUpdateDto
            {
                ReportId = report.ReportId,
                Title    = "Updated",
                Content  = "UpdatedC"
            };

            // Act
            var success = await _service.UpdateAsync(updateDto);

            // Assert
            Assert.True(success);
            var updated = await _db.Reports.FindAsync(report.ReportId);
            Assert.Equal("Updated", updated!.Title);
            Assert.Equal("UpdatedC", updated.Content);
        }

        /// <summary>
        /// Ensures UpdateAsync returns false when trying to update a non-existing report.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var updateDto = new ReportUpdateDto
            {
                ReportId = 999,
                Title    = "X",
                Content  = "Y"
            };

            var success = await _service.UpdateAsync(updateDto);

            Assert.False(success);
        }

        /// <summary>
        /// Ensures DeleteAsync removes an existing report from the database.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveReport_WhenExists()
        {
            // Arrange
            _db.Users.Add(new User { UserId = 5, FirstName = "User5", LastName = "" });
            var report = new Report
            {
                Title         = "Del",
                Content       = "DelC",
                UserId        = 5,
                GeneratedDate = DateTime.UtcNow
            };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            // Act
            var success = await _service.DeleteAsync(report.ReportId);

            // Assert
            Assert.True(success);
            Assert.False(await _db.Reports.AnyAsync(r => r.ReportId == report.ReportId));
        }

        /// <summary>
        /// Ensures DeleteAsync returns false when trying to delete a non-existing report.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var success = await _service.DeleteAsync(999);
            Assert.False(success);
        }
    }
}
