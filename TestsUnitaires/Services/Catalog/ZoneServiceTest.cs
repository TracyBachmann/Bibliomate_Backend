using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace TestsUnitaires.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="ZoneService"/> validating CRUD operations.
    /// </summary>
    public class ZonesServiceTests
    {
        private readonly ZoneService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Sets up the in-memory database context, encryption service, and ZoneService.
        /// </summary>
        public ZonesServiceTests(ITestOutputHelper output)
        {
            _output = output;

            // 1) Build in-memory EF Core options
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // 2) Provide a 32-byte Base64 key for EncryptionService
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("12345678901234567890123456789012")
            );
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = base64Key
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            // 3) Instantiate DbContext with EncryptionService
            _db = new BiblioMateDbContext(options, encryptionService);

            // 4) Instantiate ZoneService under test
            _service = new ZoneService(_db);
        }

        /// <summary>
        /// Verifies that CreateAsync adds a new zone to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddZone()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            // Arrange
            var dto = new ZoneCreateDto
            {
                FloorNumber = 1,
                AisleCode   = "A",
                Description = "Littérature jeunesse et albums illustrés"
            };

            // Act
            var result = await _service.CreateAsync(dto, _ct);

            _output.WriteLine($"Created Zone: {result.Description}");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Description, result.Description);
            Assert.True(await _db.Zones.AnyAsync(z => z.Description == dto.Description, _ct));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        /// <summary>
        /// Verifies that GetAllAsync returns all zones (paginated).
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllZones()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            // Arrange
            _db.Zones.AddRange(
                new Zone { FloorNumber = 1, AisleCode = "B", Description = "Romans et nouvelles" },
                new Zone { FloorNumber = 2, AisleCode = "C", Description = "Bandes dessinées" }
            );
            await _db.SaveChangesAsync(_ct);

            // Act
            var zones = (await _service.GetAllAsync(1, 10, _ct)).ToList();

            _output.WriteLine($"Found Zones Count: {zones.Count}");

            // Assert
            Assert.Equal(2, zones.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the zone DTO when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnZone_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            // Arrange
            var zone = new Zone
            {
                FloorNumber = 0,
                AisleCode   = "D",
                Description = "Sciences humaines"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync(_ct);

            // Act
            var dto = await _service.GetByIdAsync(zone.ZoneId, _ct);

            _output.WriteLine($"Found Zone: {dto?.Description}");

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(zone.Description, dto.Description);

            _output.WriteLine("=== GetByIdAsync (exists): END ===");
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when the zone does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            // Act
            var dto = await _service.GetByIdAsync(999, _ct);

            _output.WriteLine($"Result: {dto}");

            // Assert
            Assert.Null(dto);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        /// <summary>
        /// Verifies that UpdateAsync updates an existing zone and returns true.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyZone_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            // Arrange
            var zone = new Zone
            {
                FloorNumber = 0,
                AisleCode   = "E",
                Description = "Ancienne description"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync(_ct);

            var dto = new ZoneUpdateDto
            {
                ZoneId      = zone.ZoneId,
                FloorNumber = 1,
                AisleCode   = "E",
                Description = "Nouvelle description"
            };

            // Act
            var success = await _service.UpdateAsync(zone.ZoneId, dto, _ct);
            var updated = await _db.Zones.FindAsync(new object[] { zone.ZoneId }, _ct);

            _output.WriteLine($"Success: {success}, Updated Description: {updated?.Description}");

            // Assert
            Assert.True(success);
            Assert.Equal("Nouvelle description", updated?.Description);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        /// <summary>
        /// Verifies that UpdateAsync returns false when the zone does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            // Arrange
            var dto = new ZoneUpdateDto
            {
                ZoneId      = 999,
                FloorNumber = 1,
                AisleCode   = "Z",
                Description = "Ne devrait pas exister"
            };

            // Act
            var success = await _service.UpdateAsync(999, dto, _ct);

            _output.WriteLine($"Success: {success}");

            // Assert
            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        /// <summary>
        /// Verifies that DeleteAsync removes an existing zone and returns true.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveZone_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            // Arrange
            var zone = new Zone
            {
                FloorNumber = 1,
                AisleCode   = "F",
                Description = "Zone à supprimer"
            };
            _db.Zones.Add(zone);
            await _db.SaveChangesAsync(_ct);

            // Act
            var success = await _service.DeleteAsync(zone.ZoneId, _ct);

            _output.WriteLine($"Success: {success}");

            // Assert
            Assert.True(success);
            Assert.False(await _db.Zones.AnyAsync(z => z.ZoneId == zone.ZoneId, _ct));

            _output.WriteLine("=== DeleteAsync (success): END ===");
        }

        /// <summary>
        /// Verifies that DeleteAsync returns false when the zone does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (fail): START ===");

            // Act
            var success = await _service.DeleteAsync(999, _ct);

            _output.WriteLine($"Success: {success}");

            // Assert
            Assert.False(success);

            _output.WriteLine("=== DeleteAsync (fail): END ===");
        }
    }
}