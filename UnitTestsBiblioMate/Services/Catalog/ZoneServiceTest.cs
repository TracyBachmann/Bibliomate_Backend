using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="ZoneService"/>.
    /// Verifies all CRUD operations using EF Core InMemory provider.
    /// </summary>
    public class ZonesServiceTests
    {
        private readonly ZoneService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes an in-memory EF Core database, sets up encryption,
        /// and instantiates a <see cref="ZoneService"/> for testing.
        /// </summary>
        public ZonesServiceTests(ITestOutputHelper output)
        {
            _output = output;

            // Configure EF Core InMemory provider
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // EncryptionService dependency for DbContext
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("12345678901234567890123456789012")
            );
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Encryption:Key"] = base64Key })
                .Build();
            var encryption = new EncryptionService(config);

            _db = new BiblioMateDbContext(options, encryption);
            _service = new ZoneService(_db);
        }

        // ----------------- Create -----------------

        /// <summary>
        /// CreateAsync should add a new zone and persist it in the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddZone()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new ZoneCreateDto
            {
                Name        = "Zone A",
                FloorNumber = 1,
                AisleCode   = "A",
                Description = "Children's literature and illustrated albums"
            };

            var result = await _service.CreateAsync(dto, _ct);

            _output.WriteLine($"Created Zone: {result.ZoneId} - {result.Name}");

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.True(await _db.Zones.AnyAsync(z => z.ZoneId == result.ZoneId && z.Name == "Zone A", _ct));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        // ----------------- Read All -----------------

        /// <summary>
        /// GetAllAsync should return all zones with pagination applied.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllZones_Paginated()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.Zones.AddRange(
                new Zone { Name = "Z1", FloorNumber = 1, AisleCode = "B", Description = "Novels and short stories" },
                new Zone { Name = "Z2", FloorNumber = 2, AisleCode = "C", Description = "Comics" }
            );
            await _db.SaveChangesAsync(_ct);

            var zones = (await _service.GetAllAsync(page: 1, pageSize: 10, _ct)).ToList();

            _output.WriteLine($"Found Zones Count: {zones.Count}");

            Assert.Equal(2, zones.Count);
            Assert.Contains(zones, z => z.Name == "Z1");
            Assert.Contains(zones, z => z.Name == "Z2");

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        // ----------------- Read by Id -----------------

        /// <summary>
        /// GetByIdAsync should return a zone when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnZone_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var zone = new Zone
            {
                Name        = "Sciences",
                FloorNumber = 0,
                AisleCode   = "D",
                Description = "Human sciences"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync(_ct);

            var dto = await _service.GetByIdAsync(zone.ZoneId, _ct);

            _output.WriteLine($"Found Zone: {dto?.ZoneId} - {dto?.Name}");

            Assert.NotNull(dto);
            Assert.Equal(zone.ZoneId, dto!.ZoneId);
            Assert.Equal(zone.Name, dto.Name);
            Assert.Equal(zone.Description, dto.Description);

            _output.WriteLine("=== GetByIdAsync (exists): END ===");
        }

        /// <summary>
        /// GetByIdAsync should return null when the zone does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            var dto = await _service.GetByIdAsync(999, _ct);

            _output.WriteLine($"Result: {(dto is null ? "null" : "not null")}");

            Assert.Null(dto);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        // ----------------- Update -----------------

        /// <summary>
        /// UpdateAsync should modify an existing zone when found.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyZone_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var zone = new Zone
            {
                Name        = "Zone E",
                FloorNumber = 0,
                AisleCode   = "E",
                Description = "Old description"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync(_ct);

            var dto = new ZoneUpdateDto
            {
                ZoneId      = zone.ZoneId,
                Name        = "Zone E - Updated",
                FloorNumber = 1,
                AisleCode   = "E",
                Description = "New description"
            };

            var success = await _service.UpdateAsync(zone.ZoneId, dto, _ct);
            var updated = await _db.Zones.FindAsync(new object[] { zone.ZoneId }, _ct);

            _output.WriteLine($"Success: {success}, Updated: {updated?.Name} - {updated?.Description}");

            Assert.True(success);
            Assert.Equal(dto.Name, updated!.Name);
            Assert.Equal(dto.Description, updated.Description);
            Assert.Equal(dto.FloorNumber, updated.FloorNumber);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        /// <summary>
        /// UpdateAsync should return false when the zone does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new ZoneUpdateDto
            {
                ZoneId      = 999,
                Name        = "Should not exist",
                FloorNumber = 1,
                AisleCode   = "Z",
                Description = "Should not exist"
            };

            var success = await _service.UpdateAsync(999, dto, _ct);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        // ----------------- Delete -----------------

        /// <summary>
        /// DeleteAsync should remove an existing zone.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveZone_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var zone = new Zone
            {
                Name        = "Zone F",
                FloorNumber = 1,
                AisleCode   = "F",
                Description = "Zone to be deleted"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync(_ct);

            var success = await _service.DeleteAsync(zone.ZoneId, _ct);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.Zones.AnyAsync(z => z.ZoneId == zone.ZoneId, _ct));

            _output.WriteLine("=== DeleteAsync (success): END ===");
        }

        /// <summary>
        /// DeleteAsync should return false when the zone does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (fail): START ===");

            var success = await _service.DeleteAsync(999, _ct);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== DeleteAsync (fail): END ===");
        }
    }
}
