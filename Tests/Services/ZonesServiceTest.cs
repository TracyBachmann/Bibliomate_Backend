using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using Xunit.Abstractions;

namespace Tests.Services
{
    public class ZonesServiceTests
    {
        private readonly ZoneService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public ZonesServiceTests(ITestOutputHelper output)
        {
            _output = output;

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);

            _service = new ZoneService(_db);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddZone()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new ZoneCreateDto
            {
                FloorNumber = 1,
                AisleCode = "A",
                Description = "Littérature jeunesse et albums illustrés"
            };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created Zone: {result.Description}");

            Assert.NotNull(result);
            Assert.Equal(dto.Description, result.Description);
            Assert.True(await _db.Zones.AnyAsync(z => z.Description == dto.Description));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllZones()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.Zones.Add(new Zone { FloorNumber = 1, AisleCode = "B", Description = "Romans et nouvelles" });
            _db.Zones.Add(new Zone { FloorNumber = 2, AisleCode = "C", Description = "Bandes dessinées" });
            await _db.SaveChangesAsync();

            var zones = (await _service.GetAllAsync(page: 1, pageSize: 10)).ToList();

            _output.WriteLine($"Found Zones Count: {zones.Count}");

            Assert.Equal(2, zones.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnZone_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var zone = new Zone { FloorNumber = 0, AisleCode = "D", Description = "Sciences humaines" };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(zone.ZoneId);

            _output.WriteLine($"Found Zone: {dto?.Description}");

            Assert.NotNull(dto);
            Assert.Equal(zone.Description, dto.Description);

            _output.WriteLine("=== GetByIdAsync (exists): END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            var dto = await _service.GetByIdAsync(999);

            _output.WriteLine($"Result: {dto}");

            Assert.Null(dto);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyZone_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var zone = new Zone
            {
                FloorNumber = 0,
                AisleCode = "E",
                Description = "Ancienne description"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync();

            var dto = new ZoneUpdateDto
            {
                ZoneId = zone.ZoneId,
                FloorNumber = 1,
                AisleCode = "E",
                Description = "Nouvelle description"
            };

            var success = await _service.UpdateAsync(zone.ZoneId, dto);

            _output.WriteLine($"Success: {success}");
            _output.WriteLine($"Updated Description: {(await _db.Zones.FindAsync(zone.ZoneId))?.Description}");

            Assert.True(success);
            Assert.Equal("Nouvelle description", (await _db.Zones.FindAsync(zone.ZoneId))?.Description);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new ZoneUpdateDto
            {
                ZoneId = 999,
                FloorNumber = 1,
                AisleCode = "Z",
                Description = "Ne devrait pas exister"
            };

            var success = await _service.UpdateAsync(999, dto);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveZone_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var zone = new Zone
            {
                FloorNumber = 1,
                AisleCode = "F",
                Description = "Zone à supprimer"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(zone.ZoneId);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.Zones.AnyAsync(z => z.ZoneId == zone.ZoneId));

            _output.WriteLine("=== DeleteAsync (success): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (fail): START ===");

            var success = await _service.DeleteAsync(999);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== DeleteAsync (fail): END ===");
        }
    }
}
