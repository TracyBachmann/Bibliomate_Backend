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
    /// Uses EF Core InMemory provider to test persistence and CRUD behavior
    /// without a real database.
    /// </summary>
    public class ShelfLevelServiceTest
    {
        private readonly ShelfLevelService _service;
        private readonly BiblioMateDbContext _db;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes the in-memory test context with encryption support,
        /// seeds a <see cref="Shelf"/> entity (required by FK),
        /// and sets up the service under test.
        /// </summary>
        public ShelfLevelServiceTest()
        {
            // 1) Configure EF Core to use an isolated in-memory database for each test run
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // 2) Provide a 32-byte encryption key required by BiblioMateDbContext
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

            // 4) Seed a Shelf so that ShelfLevel foreign keys are valid
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

            // 5) Instantiate the ShelfLevelService being tested
            _service = new ShelfLevelService(_db);
        }

        /// <summary>
        /// Helper method to retrieve the seeded ShelfId.
        /// </summary>
        private int GetShelfId() => _db.Shelves.First().ShelfId;

        // -------------------- CREATE --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.CreateAsync"/> inserts a new ShelfLevel
        /// and returns the correct DTO projection.
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
            Assert.Equal(dto.ShelfId, result.ShelfId);
            Assert.Equal(dto.MaxHeight, result.MaxHeight);
            Assert.Equal(dto.Capacity, result.Capacity);
            Assert.Equal(dto.CurrentLoad, result.CurrentLoad);

            // Verify persistence
            Assert.True(await _db.ShelfLevels
                .AnyAsync(sl => sl.ShelfLevelId == result.ShelfLevelId, _ct));
        }

        // -------------------- READ (ALL) --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.GetAllAsync"/> retrieves all ShelfLevels
        /// inserted in the database.
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

        // -------------------- READ (BY ID) --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.GetByIdAsync"/> returns the ShelfLevel DTO
        /// when the entity exists.
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
            Assert.Equal(level.MaxHeight, result.MaxHeight);
            Assert.Equal(level.Capacity, result.Capacity);
            Assert.Equal(level.CurrentLoad, result.CurrentLoad);
        }

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.GetByIdAsync"/> returns null
        /// when the ShelfLevel does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetByIdAsync(999, _ct);

            // Assert
            Assert.Null(result);
        }

        // -------------------- UPDATE --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.UpdateAsync"/> modifies an existing ShelfLevel
        /// and persists the changes.
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
            Assert.Equal(dto.MaxHeight, updated?.MaxHeight);
            Assert.Equal(dto.Capacity, updated?.Capacity);
            Assert.Equal(dto.CurrentLoad, updated?.CurrentLoad);
        }

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.UpdateAsync"/> returns false
        /// when the ShelfLevel does not exist.
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

        // -------------------- DELETE --------------------

        /// <summary>
        /// Ensures that <see cref="ShelfLevelService.DeleteAsync"/> removes an existing ShelfLevel
        /// and returns true.
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
        /// Ensures that <see cref="ShelfLevelService.DeleteAsync"/> returns false
        /// when the ShelfLevel does not exist.
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
