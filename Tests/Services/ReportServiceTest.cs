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
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
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
            // Seed users et rapports
            _db.Users.Add(new User { UserId = 1, Name = "User1" });
            _db.Users.Add(new User { UserId = 2, Name = "User2" });
            _db.Reports.Add(new Report { Title = "R1", Content = "C1", UserId = 1, GeneratedDate = DateTime.UtcNow });
            _db.Reports.Add(new Report { Title = "R2", Content = "C2", UserId = 2, GeneratedDate = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            var reports = (await _service.GetAllAsync()).ToList();

            Assert.Equal(2, reports.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnReport_WhenExists()
        {
            // Seed user et rapport
            var report = new Report { Title = "Test", Content = "Cont", UserId = 3, GeneratedDate = DateTime.UtcNow };
            _db.Users.Add(new User { UserId = 3, Name = "User3" });
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(report.ReportId);

            Assert.NotNull(dto);
            Assert.Equal(report.Title, dto!.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var dto = await _service.GetByIdAsync(999);
            Assert.Null(dto);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddReport()
        {
            int userId = 10;
            _db.Users.Add(new User { UserId = userId, Name = "User10" });
            await _db.SaveChangesAsync();

            var createDto = new ReportCreateDto { Title = "New", Content = "NewContent" };
            var created = await _service.CreateAsync(createDto, userId);

            Assert.NotNull(created);
            Assert.Equal(createDto.Title, created.Title);
            Assert.True(await _db.Reports.AnyAsync(r => r.ReportId == created.ReportId));
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyReport_WhenExists()
        {
            var report = new Report { Title = "Old", Content = "OldC", UserId = 4, GeneratedDate = DateTime.UtcNow };
            _db.Users.Add(new User { UserId = 4, Name = "User4" });
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            var updateDto = new ReportUpdateDto { ReportId = report.ReportId, Title = "Updated", Content = "UpdatedC" };
            var success = await _service.UpdateAsync(updateDto);

            Assert.True(success);
            var updated = await _db.Reports.FindAsync(report.ReportId);
            Assert.Equal("Updated", updated!.Title);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var updateDto = new ReportUpdateDto { ReportId = 999, Title = "X", Content = "Y" };
            var success = await _service.UpdateAsync(updateDto);
            Assert.False(success);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveReport_WhenExists()
        {
            var report = new Report { Title = "Del", Content = "DelC", UserId = 5, GeneratedDate = DateTime.UtcNow };
            _db.Users.Add(new User { UserId = 5, Name = "User5" });
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