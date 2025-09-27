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
    /// Unit tests for <see cref="ShelfService"/>.
    /// Verifies CRUD operations using the EF Core InMemory provider,
    /// which simulates a real database for persistence testing.
    /// </summary>
    public class ShelfServiceTests
    {
        private readonly ShelfService _service;
        private readonly BiblioMateDbContext _db;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes the test class with:
        /// - An in-memory EF Core context.
        /// - A valid encryption service for DbContext initialization.
        /// - Seeded Zones and Genres to satisfy foreign keys on Shelves.
        /// </summary>
        public ShelfServiceTests()
        {
            // Configure EF Core InMemory provider
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Provide a valid 32-byte encryption key
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

            // Create DbContext with encryption
            _db = new BiblioMateDbContext(options, encryptionService);

            // Seed Zones and Genres (needed for FK integrity on Shelves)
            _db.Zones.AddRange(
                new Zone { Name = "Z-1", FloorNumber = 1, AisleCode = "A", Description = "Adult section" },
                new Zone { Name = "Z-2", FloorNumber = 2, AisleCode = "B", Description = "Youth section" }
            );
            _db.Genres.AddRange(
                new Genre { Name = "Science-Fiction" },
                new Genre { Name = "Fantasy" }
            );
            _db.SaveChanges();

            // Instantiate the service under test
            _service = new ShelfService(_db);
        }

        // -------------------- CREATE --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfService.CreateAsync"/> correctly adds a new Shelf
        /// and persists it in the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddShelf()
        {
            var dto = new ShelfCreateDto
            {
                ZoneId   = _db.Zones.First().ZoneId,
                GenreId  = _db.Genres.First().GenreId,
                Name     = "Sci-Fi Shelf",
                Capacity = 50
            };

            var result = await _service.CreateAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Shelves.AnyAsync(s => s.Name == dto.Name, _ct));
        }

        // -------------------- READ (ALL) --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfService.GetAllAsync"/> returns all shelves
        /// and applies filtering by ZoneId when provided.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllShelves()
        {
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            _db.Shelves.AddRange(
                new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Shelf A", Capacity = 30 },
                new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Shelf B", Capacity = 40 }
            );
            await _db.SaveChangesAsync(_ct);

            var allShelves = (await _service.GetAllAsync(null, 1, 10, _ct)).ToList();
            var filtered   = (await _service.GetAllAsync(zoneId, 1, 10, _ct)).ToList();

            Assert.Equal(2, allShelves.Count);
            Assert.Equal(2, filtered.Count);
        }

        // -------------------- READ (BY ID) --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfService.GetByIdAsync"/> returns a Shelf DTO
        /// when the entity exists in the database.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnShelf_WhenExists()
        {
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            var shelf = new Shelf
            {
                ZoneId   = zoneId,
                GenreId  = genreId,
                Name     = "Unique Shelf",
                Capacity = 20
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync(_ct);

            var result = await _service.GetByIdAsync(shelf.ShelfId, _ct);

            Assert.NotNull(result);
            Assert.Equal(shelf.Name, result!.Name);
        }

        /// <summary>
        /// Ensures that <see cref="ShelfService.GetByIdAsync"/> returns null
        /// when the Shelf does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _service.GetByIdAsync(999, _ct);
            Assert.Null(result);
        }

        // -------------------- UPDATE --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfService.UpdateAsync"/> modifies an existing Shelf
        /// and saves the updated values in the database.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyShelf_WhenExists()
        {
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            var shelf = new Shelf
            {
                ZoneId   = zoneId,
                GenreId  = genreId,
                Name     = "Old Shelf",
                Capacity = 30
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync(_ct);

            var dto = new ShelfUpdateDto
            {
                ShelfId  = shelf.ShelfId,
                ZoneId   = zoneId,
                GenreId  = genreId,
                Name     = "Updated Shelf",
                Capacity = 60
            };

            var success = await _service.UpdateAsync(dto, _ct);

            Assert.True(success);
            var updated = await _db.Shelves.FindAsync(new object[] { shelf.ShelfId }, _ct);
            Assert.Equal("Updated Shelf", updated?.Name);
            Assert.Equal(60, updated?.Capacity);
        }

        /// <summary>
        /// Ensures that <see cref="ShelfService.UpdateAsync"/> returns false
        /// when the Shelf does not exist in the database.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var dto = new ShelfUpdateDto
            {
                ShelfId  = 999,
                ZoneId   = 1,
                GenreId  = 1,
                Name     = "Doesn't matter",
                Capacity = 10
            };

            var success = await _service.UpdateAsync(dto, _ct);
            Assert.False(success);
        }

        // -------------------- DELETE --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfService.DeleteAsync"/> deletes a Shelf
        /// when it exists and returns true.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveShelf_WhenExists()
        {
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            var shelf = new Shelf
            {
                ZoneId   = zoneId,
                GenreId  = genreId,
                Name     = "ToDelete",
                Capacity = 20
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync(_ct);

            var success = await _service.DeleteAsync(shelf.ShelfId, _ct);

            Assert.True(success);
            Assert.False(await _db.Shelves.AnyAsync(s => s.ShelfId == shelf.ShelfId, _ct));
        }

        /// <summary>
        /// Ensures that <see cref="ShelfService.DeleteAsync"/> returns false
        /// when attempting to delete a Shelf that does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var success = await _service.DeleteAsync(999, _ct);
            Assert.False(success);
        }
    }
}
