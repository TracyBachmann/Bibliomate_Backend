using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="AuthorService"/>.
    /// Verifies CRUD operations using the in-memory EF Core provider.
    /// </summary>
    public class AuthorServiceTest
    {
        private readonly AuthorService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes the test context with an in-memory EF database and encryption.
        /// </summary>
        public AuthorServiceTest(ITestOutputHelper output)
        {
            _output = output;

            // 1) Build in-memory EF options
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // 2) Provide a 32-byte Base64 key for EncryptionService
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("12345678901234567890123456789012")
            );
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = base64Key
                })
                .Build();

            // 3) Instantiate EncryptionService and DbContext
            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);

            // 4) Instantiate service under test
            _service = new AuthorService(_db);
        }

        /// <summary>
        /// Ensures that CreateAsync adds a new author.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddAuthor()
        {
            // Arrange
            var dto = new AuthorCreateDto { Name = "Test Author" };

            // Act
            var (result, _) = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Authors.AnyAsync(a => a.Name == dto.Name));
        }

        /// <summary>
        /// Ensures that GetAllAsync returns all authors.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllAuthors()
        {
            // Arrange
            _db.Authors.AddRange(
                new Author { Name = "Author 1" },
                new Author { Name = "Author 2" }
            );
            await _db.SaveChangesAsync();

            // Act
            var authors = await _service.GetAllAsync();

            // Assert
            Assert.Equal(2, authors.Count());
        }

        /// <summary>
        /// Ensures that GetByIdAsync returns the correct author when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnAuthor_WhenExists()
        {
            // Arrange
            var author = new Author { Name = "Specific Author" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            // Act
            var (result, error) = await _service.GetByIdAsync(author.AuthorId);

            // Assert
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal(author.Name, result.Name);
        }

        /// <summary>
        /// Ensures that GetByIdAsync returns an error when the author does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenNotExists()
        {
            // Act
            var (result, error) = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
            Assert.NotNull(error);
        }

        /// <summary>
        /// Ensures that UpdateAsync modifies an existing author.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyAuthor_WhenExists()
        {
            // Arrange
            var author = new Author { Name = "Old Name" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            var dto = new AuthorCreateDto { Name = "New Name" };

            // Act
            var success = await _service.UpdateAsync(author.AuthorId, dto);

            // Assert
            Assert.True(success);
            Assert.Equal("New Name",
                (await _db.Authors.FindAsync(author.AuthorId))?.Name);
        }

        /// <summary>
        /// Ensures that UpdateAsync returns false when the author does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var dto = new AuthorCreateDto { Name = "Doesn't matter" };

            // Act
            var success = await _service.UpdateAsync(999, dto);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        /// Ensures that DeleteAsync removes an existing author.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveAuthor_WhenExists()
        {
            // Arrange
            var author = new Author { Name = "ToDelete" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            // Act
            var success = await _service.DeleteAsync(author.AuthorId);

            // Assert
            Assert.True(success);
            Assert.False(await _db.Authors
                .AnyAsync(a => a.AuthorId == author.AuthorId));
        }

        /// <summary>
        /// Ensures that DeleteAsync returns false when the author does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var success = await _service.DeleteAsync(999);

            // Assert
            Assert.False(success);
        }
    }
}