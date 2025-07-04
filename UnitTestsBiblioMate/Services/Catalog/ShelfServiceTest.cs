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
    /// Unit tests for <see cref="ShelfService"/> verifying CRUD operations
    /// using the EF Core InMemory provider.
    /// </summary>
    public class ShelfServiceTests
    {
        private readonly ShelfService _service;
        private readonly BiblioMateDbContext _db;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes the in-memory test context with seeded Zones and Genres.
        /// </summary>
        public ShelfServiceTests()
        {
            // 1) Build in-memory EF options
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

            // 4) Seed Zones and Genres for foreign keys
            _db.Zones.AddRange(
                new Zone { FloorNumber = 1, AisleCode = "A", Description = "Section Adultes" },
                new Zone { FloorNumber = 2, AisleCode = "B", Description = "Section Jeunesse" }
            );
            _db.Genres.AddRange(
                new Genre { Name = "Science-Fiction" },
                new Genre { Name = "Fantasy" }
            );
            _db.SaveChanges();

            // 5) Instantiate service under test
            _service = new ShelfService(_db);
        }

        /// <summary>
        /// Verifies that CreateAsync adds a new shelf to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddShelf()
        {
            // Arrange
            var dto = new ShelfCreateDto
            {
                ZoneId   = _db.Zones.First().ZoneId,
                GenreId  = _db.Genres.First().GenreId,
                Name     = "Étagère SF",
                Capacity = 50
            };

            // Act
            var result = await _service.CreateAsync(dto, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Shelves.AnyAsync(s => s.Name == dto.Name, _ct));
        }

        /// <summary>
        /// Verifies that GetAllAsync returns all shelves, with and without filtering by zone.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllShelves()
        {
            // Arrange
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;
            _db.Shelves.AddRange(
                new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Shelf A", Capacity = 30 },
                new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Shelf B", Capacity = 40 }
            );
            await _db.SaveChangesAsync(_ct);

            // Act
            var allShelves = (await _service.GetAllAsync(null, 1, 10, _ct)).ToList();
            var filtered   = (await _service.GetAllAsync(zoneId, 1, 10, _ct)).ToList();

            // Assert
            Assert.Equal(2, allShelves.Count);
            Assert.Equal(2, filtered.Count);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the shelf when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnShelf_WhenExists()
        {
            // Arrange
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;
            var shelf   = new Shelf
            {
                ZoneId   = zoneId,
                GenreId  = genreId,
                Name     = "Unique Shelf",
                Capacity = 20
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync(_ct);

            // Act
            var result = await _service.GetByIdAsync(shelf.ShelfId, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(shelf.Name, result.Name);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when the shelf does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetByIdAsync(999, _ct);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that UpdateAsync modifies an existing shelf.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyShelf_WhenExists()
        {
            // Arrange
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;
            var shelf   = new Shelf
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

            // Act
            var success = await _service.UpdateAsync(dto, _ct);

            // Assert
            Assert.True(success);
            var updated = await _db.Shelves.FindAsync(new object[] { shelf.ShelfId }, _ct);
            Assert.Equal("Updated Shelf", updated?.Name);
            Assert.Equal(60, updated?.Capacity);
        }

        /// <summary>
        /// Verifies that UpdateAsync returns false when the shelf does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var dto = new ShelfUpdateDto
            {
                ShelfId  = 999,
                ZoneId   = 1,
                GenreId  = 1,
                Name     = "Doesn't matter",
                Capacity = 10
            };

            // Act
            var success = await _service.UpdateAsync(dto, _ct);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        /// Verifies that DeleteAsync removes an existing shelf.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveShelf_WhenExists()
        {
            // Arrange
            var zoneId  = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;
            var shelf   = new Shelf
            {
                ZoneId   = zoneId,
                GenreId  = genreId,
                Name     = "ToDelete",
                Capacity = 20
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync(_ct);

            // Act
            var success = await _service.DeleteAsync(shelf.ShelfId, _ct);

            // Assert
            Assert.True(success);
            Assert.False(await _db.Shelves.AnyAsync(s => s.ShelfId == shelf.ShelfId, _ct));
        }

        /// <summary>
        /// Verifies that DeleteAsync returns false when the shelf does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var success = await _service.DeleteAsync(999, _ct);

            // Assert
            Assert.False(success);
        }
    }
}