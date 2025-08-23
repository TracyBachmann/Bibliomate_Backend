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

        public ReportServiceTest()
        {
            // In-memory EF Core + EncryptionService (required by DbContext constructor)
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

            _db      = new BiblioMateDbContext(options, encryptionService);
            _service = new ReportService(_db);
        }

        /// <summary>
        /// GetAllAsync should return all persisted reports.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllReports()
        {
            // Arrange: seed users and two reports
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
        /// GetByIdAsync returns the DTO when the report exists.
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
        /// GetByIdAsync returns null when the report does not exist.
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
        /// CreateAsync generates report content based on monthly loan stats,
        /// saves it, and returns the created DTO.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldGenerateAndSaveReport()
        {
            // Arrange: seed a user and loans in this and last month
            const int userId = 10;
            _db.Users.Add(new User { UserId = userId, FirstName = "User10", LastName = "" });

            var now            = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // Two loans this month, one last month
            _db.Loans.Add(new Loan { LoanDate = thisMonthStart.AddDays(1), BookId = 1 });
            _db.Loans.Add(new Loan { LoanDate = thisMonthStart.AddDays(2), BookId = 2 });
            _db.Loans.Add(new Loan { LoanDate = lastMonthStart.AddDays(1), BookId = 1 });

            // Seed books for title inclusion
            _db.Books.Add(new Book { BookId = 1, Title = "Book A" });
            _db.Books.Add(new Book { BookId = 2, Title = "Book B" });

            await _db.SaveChangesAsync();

            var createDto = new ReportCreateDto { Title = "Monthly Stats" };

            // Act
            var created = await _service.CreateAsync(createDto, userId);

            // Assert
            Assert.NotNull(created);
            Assert.Equal("Monthly Stats", created.Title);
            Assert.False(string.IsNullOrWhiteSpace(created.Content));
            Assert.Contains("Loans this month: 2", created.Content);
            Assert.Contains("Loans last month: 1", created.Content);
            Assert.Contains("Book A", created.Content);
            Assert.Contains("Book B", created.Content);

            // Confirm persistence
            Assert.True(await _db.Reports.AnyAsync(r => r.ReportId == created.ReportId));
        }

        /// <summary>
        /// UpdateAsync modifies an existing report and returns true.
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
        /// UpdateAsync returns false when the report does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var updateDto = new ReportUpdateDto
            {
                ReportId = 999,
                Title    = "X",
                Content  = "Y"
            };

            // Act
            var success = await _service.UpdateAsync(updateDto);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        /// DeleteAsync removes an existing report and returns true.
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
        /// DeleteAsync returns false when the report does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var success = await _service.DeleteAsync(999);

            // Assert
            Assert.False(success);
        }
    }
}
