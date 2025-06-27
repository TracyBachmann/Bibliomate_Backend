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
    public class ShelfLevelServiceTests
    {
        private readonly ShelfLevelService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public ShelfLevelServiceTests(ITestOutputHelper output)
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

            _service = new ShelfLevelService(_db);

            // Seed a Shelf for foreign key
            var shelf = new Shelf
            {
                Name = "Test Shelf",
                ZoneId = 1,
                GenreId = 1,
                Capacity = 100,
                CurrentLoad = 0
            };
            _db.Shelves.Add(shelf);
            _db.SaveChanges();
        }

        private int GetShelfId() => _db.Shelves.First().ShelfId;

        [Fact]
        public async Task CreateAsync_ShouldAddShelfLevel()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new ShelfLevelCreateDto
            {
                LevelNumber = 1,
                ShelfId = GetShelfId(),
                MaxHeight = 35,
                Capacity = 20,
                CurrentLoad = 5
            };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created Level ID: {result.ShelfLevelId}, Shelf Name: {result.ShelfName}");

            Assert.NotNull(result);
            Assert.Equal(dto.LevelNumber, result.LevelNumber);
            Assert.Equal(dto.ShelfId, result.ShelfId);
            Assert.Equal(dto.MaxHeight, result.MaxHeight);
            Assert.Equal(dto.Capacity, result.Capacity);
            Assert.Equal(dto.CurrentLoad, result.CurrentLoad);

            Assert.True(await _db.ShelfLevels.AnyAsync(sl => sl.ShelfLevelId == result.ShelfLevelId));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllShelfLevels()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.ShelfLevels.Add(new ShelfLevel { LevelNumber = 1, ShelfId = GetShelfId(), MaxHeight = 30, Capacity = 20, CurrentLoad = 5 });
            _db.ShelfLevels.Add(new ShelfLevel { LevelNumber = 2, ShelfId = GetShelfId(), MaxHeight = 35, Capacity = 25, CurrentLoad = 10 });
            await _db.SaveChangesAsync();

            var result = (await _service.GetAllAsync(null, 1, 10)).ToList();

            _output.WriteLine($"Found Shelf Levels Count: {result.Count}");

            Assert.Equal(2, result.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnShelfLevel_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var level = new ShelfLevel { LevelNumber = 1, ShelfId = GetShelfId(), MaxHeight = 40, Capacity = 30, CurrentLoad = 15 };
            _db.ShelfLevels.Add(level);
            await _db.SaveChangesAsync();

            var result = await _service.GetByIdAsync(level.ShelfLevelId);

            _output.WriteLine($"Found Level: {result?.ShelfLevelId}, Name: {result?.ShelfName}");

            Assert.NotNull(result);
            Assert.Equal(level.LevelNumber, result.LevelNumber);
            Assert.Equal(level.MaxHeight, result.MaxHeight);
            Assert.Equal(level.Capacity, result.Capacity);
            Assert.Equal(level.CurrentLoad, result.CurrentLoad);

            _output.WriteLine("=== GetByIdAsync (exists): END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            var result = await _service.GetByIdAsync(999);

            _output.WriteLine($"Result: {result}");

            Assert.Null(result);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyShelfLevel_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var level = new ShelfLevel { LevelNumber = 1, ShelfId = GetShelfId(), MaxHeight = 30, Capacity = 20, CurrentLoad = 5 };
            _db.ShelfLevels.Add(level);
            await _db.SaveChangesAsync();

            var dto = new ShelfLevelUpdateDto
            {
                ShelfLevelId = level.ShelfLevelId,
                LevelNumber = 2,
                ShelfId = GetShelfId(),
                MaxHeight = 45,
                Capacity = 25,
                CurrentLoad = 8
            };

            var success = await _service.UpdateAsync(dto);

            _output.WriteLine($"Success: {success}");

            var updated = await _db.ShelfLevels.FindAsync(level.ShelfLevelId);

            Assert.True(success);
            Assert.Equal(dto.LevelNumber, updated?.LevelNumber);
            Assert.Equal(dto.MaxHeight, updated?.MaxHeight);
            Assert.Equal(dto.Capacity, updated?.Capacity);
            Assert.Equal(dto.CurrentLoad, updated?.CurrentLoad);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new ShelfLevelUpdateDto
            {
                ShelfLevelId = 999,
                LevelNumber = 1,
                ShelfId = GetShelfId(),
                MaxHeight = 30,
                Capacity = 20,
                CurrentLoad = 5
            };

            var success = await _service.UpdateAsync(dto);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveShelfLevel_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var level = new ShelfLevel { LevelNumber = 1, ShelfId = GetShelfId(), MaxHeight = 30, Capacity = 20, CurrentLoad = 5 };
            _db.ShelfLevels.Add(level);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(level.ShelfLevelId);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.ShelfLevels.AnyAsync(sl => sl.ShelfLevelId == level.ShelfLevelId));

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
