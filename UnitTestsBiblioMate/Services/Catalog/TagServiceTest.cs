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
    /// Unit tests for <see cref="TagService"/> validating CRUD operations.
    /// </summary>
    public class TagsServiceTests
    {
        private readonly TagService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Sets up the in-memory database context, encryption service, and TagService.
        /// </summary>
        public TagsServiceTests(ITestOutputHelper output)
        {
            _output = output;

            // 1) Build in-memory EF Core options
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

            // 4) Instantiate TagService under test
            _service = new TagService(_db);
        }

        /// <summary>
        /// Verifies that CreateAsync adds a new tag to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddTag()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            // Arrange
            var dto = new TagCreateDto { Name = "Classic" };

            // Act
            var result = await _service.CreateAsync(dto, _ct);

            _output.WriteLine($"Created Tag: {result.Name}");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Tags.AnyAsync(t => t.Name == dto.Name, _ct));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        /// <summary>
        /// Verifies that GetAllAsync returns all tags present in the database.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTags()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            // Arrange
            _db.Tags.AddRange(
                new Tag { Name = "Adventure" },
                new Tag { Name = "Coming-of-Age" }
            );
            await _db.SaveChangesAsync(_ct);

            // Act
            var tags = (await _service.GetAllAsync(_ct)).ToList();

            _output.WriteLine($"Found Tags Count: {tags.Count}");

            // Assert
            Assert.Equal(2, tags.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the tag DTO when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnTag_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            // Arrange
            var tag = new Tag { Name = "Post-apocalyptique" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(_ct);

            // Act
            var dto = await _service.GetByIdAsync(tag.TagId, _ct);

            _output.WriteLine($"Found Tag: {dto?.Name}");

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(tag.Name, dto.Name);

            _output.WriteLine("=== GetByIdAsync (exists): END ===");
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when the tag does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            // Act
            var dto = await _service.GetByIdAsync(999, _ct);

            _output.WriteLine($"Result: {dto}");

            // Assert
            Assert.Null(dto);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        /// <summary>
        /// Verifies that UpdateAsync successfully updates an existing tag.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyTag_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            // Arrange
            var tag = new Tag { Name = "Old Tag" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(_ct);

            var dto = new TagUpdateDto { TagId = tag.TagId, Name = "Updated Tag" };

            // Act
            var success = await _service.UpdateAsync(dto, _ct);
            var updated = await _db.Tags.FindAsync(new object[] { tag.TagId }, _ct);

            _output.WriteLine($"Success: {success}, Updated Name: {updated?.Name}");

            // Assert
            Assert.True(success);
            Assert.Equal("Updated Tag", updated?.Name);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        /// <summary>
        /// Verifies that UpdateAsync returns false when the tag to update does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            // Arrange
            var dto = new TagUpdateDto { TagId = 999, Name = "Doesn't matter" };

            // Act
            var success = await _service.UpdateAsync(dto, _ct);

            _output.WriteLine($"Success: {success}");

            // Assert
            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        /// <summary>
        /// Verifies that DeleteAsync removes an existing tag.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveTag_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            // Arrange
            var tag = new Tag { Name = "ToDelete" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(_ct);

            // Act
            var success = await _service.DeleteAsync(tag.TagId, _ct);

            _output.WriteLine($"Success: {success}");

            // Assert
            Assert.True(success);
            Assert.False(await _db.Tags.AnyAsync(t => t.TagId == tag.TagId, _ct));

            _output.WriteLine("=== DeleteAsync (success): END ===");
        }

        /// <summary>
        /// Verifies that DeleteAsync returns false when the tag to delete does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (fail): START ===");

            // Act
            var success = await _service.DeleteAsync(999, _ct);

            _output.WriteLine($"Success: {success}");

            // Assert
            Assert.False(success);

            _output.WriteLine("=== DeleteAsync (fail): END ===");
        }
    }
}