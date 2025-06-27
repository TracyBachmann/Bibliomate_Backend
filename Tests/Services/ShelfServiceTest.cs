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
    public class ShelfServiceTests
    {
        private readonly ShelfService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public ShelfServiceTests(ITestOutputHelper output)
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

            // Seed zones and genres for foreign keys
            _db.Zones.AddRange(
                new Zone { FloorNumber = 1, AisleCode = "A", Description = "Section Adultes" },
                new Zone { FloorNumber = 2, AisleCode = "B", Description = "Section Jeunesse" }
            );

            _db.Genres.AddRange(
                new Genre { Name = "Science-Fiction" },
                new Genre { Name = "Fantasy" }
            );

            _db.SaveChanges();

            _service = new ShelfService(_db);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddShelf()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new ShelfCreateDto
            {
                ZoneId = _db.Zones.First().ZoneId,
                GenreId = _db.Genres.First().GenreId,
                Name = "Étagère SF",
                Capacity = 50
            };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created Shelf: {result.Name}, ID: {result.ShelfId}");

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Shelves.AnyAsync(s => s.Name == dto.Name));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllShelves()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            var zoneId = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            _db.Shelves.Add(new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Shelf A", Capacity = 30 });
            _db.Shelves.Add(new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Shelf B", Capacity = 40 });
            await _db.SaveChangesAsync();

            var all = (await _service.GetAllAsync(null, 1, 10)).ToList();

            _output.WriteLine($"Found Shelves Count (no filter): {all.Count}");
            Assert.Equal(2, all.Count);

            var filtered = (await _service.GetAllAsync(zoneId, 1, 10)).ToList();

            _output.WriteLine($"Found Shelves Count (zone filter): {filtered.Count}");
            Assert.Equal(2, filtered.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnShelf_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var zoneId = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            var shelf = new Shelf { ZoneId = zoneId, GenreId = genreId, Name = "Unique Shelf", Capacity = 20 };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(shelf.ShelfId);

            _output.WriteLine($"Found Shelf: {dto?.Name}");
            Assert.NotNull(dto);
            Assert.Equal(shelf.Name, dto.Name);

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
        public async Task UpdateAsync_ShouldModifyShelf_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var zoneId = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            var shelf = new Shelf
            {
                ZoneId = zoneId,
                GenreId = genreId,
                Name = "Old Shelf",
                Capacity = 30
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync();

            var dto = new ShelfUpdateDto
            {
                ShelfId = shelf.ShelfId,
                ZoneId = zoneId,
                GenreId = genreId,
                Name = "Updated Shelf",
                Capacity = 60
            };

            var success = await _service.UpdateAsync(dto);

            _output.WriteLine($"Success: {success}");
            _output.WriteLine($"Updated Name: {(await _db.Shelves.FindAsync(shelf.ShelfId))?.Name}");

            Assert.True(success);
            Assert.Equal("Updated Shelf", (await _db.Shelves.FindAsync(shelf.ShelfId))?.Name);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new ShelfUpdateDto
            {
                ShelfId = 999,
                ZoneId = 1,
                GenreId = 1,
                Name = "Doesn't matter",
                Capacity = 10
            };

            var success = await _service.UpdateAsync(dto);

            _output.WriteLine($"Success: {success}");
            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveShelf_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var zoneId = _db.Zones.First().ZoneId;
            var genreId = _db.Genres.First().GenreId;

            var shelf = new Shelf
            {
                ZoneId = zoneId,
                GenreId = genreId,
                Name = "ToDelete",
                Capacity = 20
            };
            _db.Shelves.Add(shelf);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(shelf.ShelfId);

            _output.WriteLine($"Success: {success}");
            Assert.True(success);
            Assert.False(await _db.Shelves.AnyAsync(s => s.ShelfId == shelf.ShelfId));

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
