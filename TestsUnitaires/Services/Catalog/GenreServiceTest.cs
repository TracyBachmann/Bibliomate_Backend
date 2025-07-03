using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestsUnitaires.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="GenreService"/>.
    /// Verifies CRUD operations using an in-memory EF Core provider.
    /// </summary>
    public class GenreServiceTest
    {
        private readonly GenreService _service;
        private readonly BiblioMateDbContext _db;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes the test context with in-memory EF Core and encryption.
        /// </summary>
        public GenreServiceTest()
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

            // 4) Instantiate service under test
            _service = new GenreService(_db);
        }

        /// <summary>
        /// Verifies that CreateAsync adds a new genre to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddGenre()
        {
            // Arrange
            var dto = new GenreCreateDto { Name = "Science Fiction" };

            // Act
            var (createdDto, _) = await _service.CreateAsync(dto, _ct);

            // Assert
            Assert.NotNull(createdDto);
            Assert.Equal(dto.Name, createdDto.Name);
            Assert.True(await _db.Genres.AnyAsync(g => g.Name == dto.Name, _ct));
        }

        /// <summary>
        /// Verifies that GetAllAsync returns all existing genres.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllGenres()
        {
            // Arrange
            _db.Genres.AddRange(
                new Genre { Name = "Genre 1" },
                new Genre { Name = "Genre 2" }
            );
            await _db.SaveChangesAsync(_ct);

            // Act
            var genres = (await _service.GetAllAsync(_ct)).ToList();

            // Assert
            Assert.Equal(2, genres.Count);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the correct genre when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnGenre_WhenExists()
        {
            // Arrange
            var genre = new Genre { Name = "Specific Genre" };
            _db.Genres.Add(genre);
            await _db.SaveChangesAsync(_ct);

            // Act
            var (dto, error) = await _service.GetByIdAsync(genre.GenreId, _ct);

            // Assert
            Assert.Null(error);
            Assert.NotNull(dto);
            Assert.Equal(genre.Name, dto.Name);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns a NotFoundResult when the genre does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenNotExists()
        {
            // Act
            var (dto, error) = await _service.GetByIdAsync(999, _ct);

            // Assert
            Assert.Null(dto);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(error);
        }

        /// <summary>
        /// Verifies that UpdateAsync successfully updates an existing genre.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyGenre_WhenExists()
        {
            // Arrange
            var genre = new Genre { Name = "Old Genre" };
            _db.Genres.Add(genre);
            await _db.SaveChangesAsync(_ct);

            var dto = new GenreUpdateDto { Name = "Updated Genre" };

            // Act
            var success = await _service.UpdateAsync(genre.GenreId, dto, _ct);

            // Assert
            Assert.True(success);
            var updated = await _db.Genres.FindAsync(new object[] { genre.GenreId }, _ct);
            Assert.Equal("Updated Genre", updated?.Name);
        }

        /// <summary>
        /// Verifies that UpdateAsync returns false when the genre to update does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var dto = new GenreUpdateDto { Name = "Doesn't matter" };

            // Act
            var success = await _service.UpdateAsync(999, dto, _ct);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        /// Verifies that DeleteAsync removes an existing genre.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveGenre_WhenExists()
        {
            // Arrange
            var genre = new Genre { Name = "ToDelete" };
            _db.Genres.Add(genre);
            await _db.SaveChangesAsync(_ct);

            // Act
            var success = await _service.DeleteAsync(genre.GenreId, _ct);

            // Assert
            Assert.True(success);
            Assert.False(await _db.Genres.AnyAsync(g => g.GenreId == genre.GenreId, _ct));
        }

        /// <summary>
        /// Verifies that DeleteAsync returns false when the genre to delete does not exist.
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