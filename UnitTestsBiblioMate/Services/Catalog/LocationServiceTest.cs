using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTestsBiblioMate.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="LocationService"/>.
    /// Covers:
    /// <list type="bullet">
    ///   <item><description>Retrieving distinct floors, aisles, shelves, and shelf levels</description></item>
    ///   <item><description>Validating ordering and filtering by hierarchy</description></item>
    ///   <item><description>Ensuring (creating or reusing) location hierarchy</description></item>
    /// </list>
    /// Uses EF Core InMemory provider to simulate persistence.
    /// </summary>
    public class LocationServiceTest : IDisposable
    {
        private readonly BiblioMateDbContext _db;
        private readonly LocationService _service;

        /// <summary>
        /// Initializes an in-memory DbContext and a LocationService instance.
        /// Also seeds a default <see cref="Genre"/> because Shelf requires a valid GenreId.
        /// </summary>
        public LocationServiceTest()
        {
            // EF InMemory DbContext
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Encryption key required by DbContext constructor
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = base64Key
                })
                .Build();

            var encryption = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryption);
            _service = new LocationService(_db);

            // Default genre (GenreId = 1) for Shelf FK consistency
            _db.Genres.Add(new Genre { GenreId = 1, Name = "Neutral" });
            _db.SaveChanges();
        }

        /// <summary>
        /// Disposes the in-memory database after each test.
        /// </summary>
        public void Dispose() => _db.Dispose();

        // ---------- helpers ----------

        /// <summary>
        /// Seeds a minimal hierarchy of zones, shelves, and shelf levels.
        /// </summary>
        private (Zone z1, Zone z2, Shelf s11, Shelf s12, Shelf s21, ShelfLevel l11a, ShelfLevel l11b) SeedBasic()
        {
            var z1 = _db.Zones.Add(new Zone { Name = "Z-1-A", FloorNumber = 1, AisleCode = "A" }).Entity;
            var z2 = _db.Zones.Add(new Zone { Name = "Z-2-A", FloorNumber = 2, AisleCode = "A" }).Entity;
            _db.SaveChanges();

            var s11 = _db.Shelves.Add(new Shelf { ZoneId = z1.ZoneId, Name = "Shelf-1-1", GenreId = 1, Capacity = 0, CurrentLoad = 0 }).Entity;
            var s12 = _db.Shelves.Add(new Shelf { ZoneId = z1.ZoneId, Name = "Shelf-1-2", GenreId = 1, Capacity = 0, CurrentLoad = 0 }).Entity;
            var s21 = _db.Shelves.Add(new Shelf { ZoneId = z2.ZoneId, Name = "Shelf-2-1", GenreId = 1, Capacity = 0, CurrentLoad = 0 }).Entity;
            _db.SaveChanges();

            var l11a = _db.ShelfLevels.Add(new ShelfLevel { ShelfId = s11.ShelfId, LevelNumber = 1, Capacity = 0, CurrentLoad = 0, MaxHeight = 0 }).Entity;
            var l11b = _db.ShelfLevels.Add(new ShelfLevel { ShelfId = s11.ShelfId, LevelNumber = 3, Capacity = 0, CurrentLoad = 0, MaxHeight = 0 }).Entity;
            _db.SaveChanges();

            return (z1, z2, s11, s12, s21, l11a, l11b);
        }

        // ---------- floors ----------

        /// <summary>
        /// GetFloorsAsync should return distinct floor numbers sorted ascending.
        /// </summary>
        [Fact]
        public async Task GetFloorsAsync_ReturnsDistinctSortedFloors()
        {
            _db.Zones.AddRange(
                new Zone { Name = "Z-2-A", FloorNumber = 2, AisleCode = "A" },
                new Zone { Name = "Z-1-A", FloorNumber = 1, AisleCode = "A" },
                new Zone { Name = "Z-2-B", FloorNumber = 2, AisleCode = "B" }
            );
            await _db.SaveChangesAsync();

            var floors = (await _service.GetFloorsAsync()).Select(f => f.FloorNumber).ToList();

            Assert.Equal(new[] { 1, 2 }, floors);
        }

        // ---------- aisles ----------

        /// <summary>
        /// GetAislesAsync should filter by floor and return distinct aisle codes sorted alphabetically.
        /// </summary>
        [Fact]
        public async Task GetAislesAsync_FiltersByFloor_AndReturnsDistinctSorted()
        {
            _db.Zones.AddRange(
                new Zone { Name = "Z-1-A", FloorNumber = 1, AisleCode = "A" },
                new Zone { Name = "Z-1-B", FloorNumber = 1, AisleCode = "B" },
                new Zone { Name = "Z-2-A", FloorNumber = 2, AisleCode = "A" }
            );
            await _db.SaveChangesAsync();

            var aisles = (await _service.GetAislesAsync(1)).Select(a => a.AisleCode).ToList();

            Assert.Equal(new[] { "A", "B" }, aisles);
        }

        // ---------- shelves ----------

        /// <summary>
        /// GetShelvesAsync should return shelves filtered by floor and aisle,
        /// ordered by shelf name.
        /// </summary>
        [Fact]
        public async Task GetShelvesAsync_FiltersByFloorAndAisle_AndOrdersByName()
        {
            var z1 = _db.Zones.Add(new Zone { Name = "Z-1-A", FloorNumber = 1, AisleCode = "A" }).Entity;
            var z2 = _db.Zones.Add(new Zone { Name = "Z-1-B", FloorNumber = 1, AisleCode = "B" }).Entity;
            await _db.SaveChangesAsync();

            _db.Shelves.AddRange(
                new Shelf { ZoneId = z1.ZoneId, Name = "Shelf-B", GenreId = 1, Capacity = 0, CurrentLoad = 0 },
                new Shelf { ZoneId = z1.ZoneId, Name = "Shelf-A", GenreId = 1, Capacity = 0, CurrentLoad = 0 },
                new Shelf { ZoneId = z2.ZoneId, Name = "Shelf-X", GenreId = 1, Capacity = 0, CurrentLoad = 0 } // different aisle
            );
            await _db.SaveChangesAsync();

            var shelves = (await _service.GetShelvesAsync(1, "A")).ToList();

            Assert.Equal(2, shelves.Count);
            Assert.Equal(new[] { "Shelf-A", "Shelf-B" }, shelves.Select(s => s.Name).ToArray());
        }

        // ---------- levels ----------

        /// <summary>
        /// GetLevelsAsync should return levels for a shelf ordered by <see cref="ShelfLevel.LevelNumber"/>.
        /// </summary>
        [Fact]
        public async Task GetLevelsAsync_ReturnsOrderedLevelsForShelf()
        {
            var (_, _, s11, _, _, l1, l3) = SeedBasic();
            var l2 = _db.ShelfLevels.Add(new ShelfLevel { ShelfId = s11.ShelfId, LevelNumber = 2, Capacity = 0, CurrentLoad = 0, MaxHeight = 0 }).Entity;
            await _db.SaveChangesAsync();

            var levels = (await _service.GetLevelsAsync(s11.ShelfId)).Select(l => l.LevelNumber).ToList();

            Assert.Equal(new[] { 1, 2, 3 }, levels);
        }

        // ---------- ensure ----------

        /// <summary>
        /// EnsureAsync should create Zone, Shelf, and ShelfLevel when they do not exist.
        /// </summary>
        [Fact]
        public async Task EnsureAsync_CreatesZoneShelfLevel_WhenMissing()
        {
            var dto = new LocationEnsureDto
            {
                FloorNumber = 3,
                AisleCode   = "C1",
                ShelfName   = "S-10",
                LevelNumber = 2
            };

            var res = await _service.EnsureAsync(dto);

            Assert.True(res.ZoneId > 0);
            Assert.True(res.ShelfId > 0);
            Assert.True(res.ShelfLevelId > 0);
            Assert.Equal(3, res.FloorNumber);
            Assert.Equal("C1", res.AisleCode);
            Assert.Equal("S-10", res.ShelfName);
            Assert.Equal(2, res.LevelNumber);

            // Verify persistence in database
            Assert.NotNull(await _db.Zones.FindAsync(res.ZoneId));
            Assert.NotNull(await _db.Shelves.FindAsync(res.ShelfId));
            Assert.NotNull(await _db.ShelfLevels.FindAsync(res.ShelfLevelId));
        }

        /// <summary>
        /// EnsureAsync should reuse existing entities if the same location is ensured twice.
        /// </summary>
        [Fact]
        public async Task EnsureAsync_ReusesExistingEntities_OnSecondCall()
        {
            // First call -> creates new entities
            var first = await _service.EnsureAsync(new LocationEnsureDto
            {
                FloorNumber = 1,
                AisleCode   = "A",
                ShelfName   = "R-01",
                LevelNumber = 3
            });

            // Second call with same values -> should reuse existing entities
            var second = await _service.EnsureAsync(new LocationEnsureDto
            {
                FloorNumber = 1,
                AisleCode   = "A",
                ShelfName   = "R-01",
                LevelNumber = 3
            });

            Assert.Equal(first.ZoneId,       second.ZoneId);
            Assert.Equal(first.ShelfId,      second.ShelfId);
            Assert.Equal(first.ShelfLevelId, second.ShelfLevelId);
        }
    }
}
