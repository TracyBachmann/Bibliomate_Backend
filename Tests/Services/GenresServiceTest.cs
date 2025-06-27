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
    public class GenresServiceTests
    {
        private readonly GenreService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public GenresServiceTests(ITestOutputHelper output)
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

            _service = new GenreService(_db);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddGenre()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new GenreCreateDto { Name = "Science-fiction" };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created Genre: {result.Name}");

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Genres.AnyAsync(g => g.Name == dto.Name));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllGenres()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.Genres.Add(new Genre { Name = "Fantasy" });
            _db.Genres.Add(new Genre { Name = "Horreur" });
            await _db.SaveChangesAsync();

            var genres = (await _service.GetAllAsync()).ToList();

            _output.WriteLine($"Found Genres Count: {genres.Count}");

            Assert.Equal(2, genres.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnGenre_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var genre = new Genre { Name = "Mystère" };
            _db.Genres.Add(genre);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(genre.GenreId);

            _output.WriteLine($"Found Genre: {dto?.Name}");

            Assert.NotNull(dto);
            Assert.Equal(genre.Name, dto.Name);

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
        public async Task UpdateAsync_ShouldModifyGenre_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var genre = new Genre { Name = "Ancien Genre" };
            _db.Genres.Add(genre);
            await _db.SaveChangesAsync();

            var dto = new GenreCreateDto { Name = "Genre Mis à Jour" };
            var success = await _service.UpdateAsync(genre.GenreId, dto);

            _output.WriteLine($"Success: {success}");
            _output.WriteLine($"Updated Name: {(await _db.Genres.FindAsync(genre.GenreId))?.Name}");

            Assert.True(success);
            Assert.Equal("Genre Mis à Jour", (await _db.Genres.FindAsync(genre.GenreId))?.Name);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new GenreCreateDto { Name = "Inutile" };
            var success = await _service.UpdateAsync(999, dto);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveGenre_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var genre = new Genre { Name = "À Supprimer" };
            _db.Genres.Add(genre);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(genre.GenreId);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.Genres.AnyAsync(g => g.GenreId == genre.GenreId));

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
