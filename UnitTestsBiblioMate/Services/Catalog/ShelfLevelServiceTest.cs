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
    /// Unit tests for <see cref="ShelfLevelService"/>.
    /// Verifies CRUD operations using an in-memory EF Core provider.
    /// </summary>
    public class ShelfLevelServiceTest
    {
        private readonly ShelfLevelService _service;
        private readonly BiblioMateDbContext _db;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes the in-memory test context with encryption and seeds a Shelf.
        /// </summary>
        public ShelfLevelServiceTest()
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

            // 4) Seed a Shelf for the foreign key
            var shelf = new Shelf
            {
                Name        = "Test Shelf",
                ZoneId      = 1,
                GenreId     = 1,
                Capacity    = 100,
                CurrentLoad = 0
            };
            _db.Shelves.Add(shelf);
            _db.SaveChanges();

            // 5) Instantiate service under test
            _service = new ShelfLevelService(_db);
        }

        private int GetShelfId() => _db.Shelves.First().ShelfId;

        /// <summary>
        /// Verifies that CreateAsync adds a new ShelfLevel.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddShelfLevel()
        {
            // Arrange
            var dto = new ShelfLevelCreateDto
            {
                LevelNumber = 1,
                ShelfId     = GetShelfId(),
                MaxHeight   = 35,
                Capacity    = 20,
                CurrentLoad = 5
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.LevelNumber, result.LevelNumber);
            Assert.Equal(dto.ShelfId,     result.ShelfId);
            Assert.Equal(dto.MaxHeight,   result.MaxHeight);
            Assert.Equal(dto.Capacity,    result.Capacity);
            Assert.Equal(dto.CurrentLoad, result.CurrentLoad);
            Assert.True(await _db.ShelfLevels
                .AnyAsync(sl => sl.ShelfLevelId == result.ShelfLevelId, _ct));
        }

        /// <summary>
        /// Verifies that GetAllAsync returns all ShelfLevels.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllShelfLevels()
        {
            // Arrange
            _db.ShelfLevels.AddRange(
                new ShelfLevel { LevelNumber = 1, ShelfId = GetShelfId(), MaxHeight = 30, Capacity = 20, CurrentLoad = 5 },
                new ShelfLevel { LevelNumber = 2, ShelfId = GetShelfId(), MaxHeight = 35, Capacity = 25, CurrentLoad = 10 }
            );
            await _db.SaveChangesAsync(_ct);

            // Act
            var result = (await _service.GetAllAsync(null, 1, 10, _ct)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the ShelfLevel when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnShelfLevel_WhenExists()
        {
            // Arrange
            var level = new ShelfLevel
            {
                LevelNumber = 1,
                ShelfId     = GetShelfId(),
                MaxHeight   = 40,
                Capacity    = 30,
                CurrentLoad = 15
            };
            _db.ShelfLevels.Add(level);
            await _db.SaveChangesAsync(_ct);

            // Act
            var result = await _service.GetByIdAsync(level.ShelfLevelId, _ct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(level.LevelNumber, result.LevelNumber);
            Assert.Equal(level.MaxHeight,   result.MaxHeight);
            Assert.Equal(level.Capacity,    result.Capacity);
            Assert.Equal(level.CurrentLoad, result.CurrentLoad);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when the ShelfLevel does not exist.
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
        /// Verifies that UpdateAsync modifies an existing ShelfLevel.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyShelfLevel_WhenExists()
        {
            // Arrange
            var level = new ShelfLevel
            {
                LevelNumber = 1,
                ShelfId     = GetShelfId(),
                MaxHeight   = 30,
                Capacity    = 20,
                CurrentLoad = 5
            };
            _db.ShelfLevels.Add(level);
            await _db.SaveChangesAsync(_ct);

            var dto = new ShelfLevelUpdateDto
            {
                ShelfLevelId = level.ShelfLevelId,
                LevelNumber  = 2,
                ShelfId      = GetShelfId(),
                MaxHeight    = 45,
                Capacity     = 25,
                CurrentLoad  = 8
            };

            // Act
            var success = await _service.UpdateAsync(dto, _ct);

            // Assert
            Assert.True(success);
            var updated = await _db.ShelfLevels.FindAsync(
                new object[] { level.ShelfLevelId }, _ct);
            Assert.Equal(dto.LevelNumber, updated?.LevelNumber);
            Assert.Equal(dto.MaxHeight,   updated?.MaxHeight);
            Assert.Equal(dto.Capacity,    updated?.Capacity);
            Assert.Equal(dto.CurrentLoad, updated?.CurrentLoad);
        }

        /// <summary>
        /// Verifies that UpdateAsync returns false when the ShelfLevel does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var dto = new ShelfLevelUpdateDto
            {
                ShelfLevelId = 999,
                LevelNumber  = 1,
                ShelfId      = GetShelfId(),
                MaxHeight    = 30,
                Capacity     = 20,
                CurrentLoad  = 5
            };

            // Act
            var success = await _service.UpdateAsync(dto, _ct);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        /// Verifies that DeleteAsync removes an existing ShelfLevel.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveShelfLevel_WhenExists()
        {
            // Arrange
            var level = new ShelfLevel
            {
                LevelNumber = 1,
                ShelfId     = GetShelfId(),
                MaxHeight   = 30,
                Capacity    = 20,
                CurrentLoad = 5
            };
            _db.ShelfLevels.Add(level);
            await _db.SaveChangesAsync(_ct);

            // Act
            var success = await _service.DeleteAsync(level.ShelfLevelId, _ct);

            // Assert
            Assert.True(success);
            Assert.False(await _db.ShelfLevels
                .AnyAsync(sl => sl.ShelfLevelId == level.ShelfLevelId, _ct));
        }

        /// <summary>
        /// Verifies that DeleteAsync returns false when the ShelfLevel does not exist.
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