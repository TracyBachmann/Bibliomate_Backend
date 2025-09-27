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
    /// Unit tests for <see cref="TagService"/>.
    /// Verifies CRUD operations, <c>SearchAsync</c>, and <c>EnsureAsync</c>
    /// using the EF Core InMemory provider.
    /// </summary>
    public class TagServiceTests : IDisposable
    {
        private readonly BiblioMateDbContext _db;
        private readonly TagService _service;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes an in-memory EF Core context with encryption service
        /// and prepares a TagService instance for testing.
        /// </summary>
        public TagServiceTests()
        {
            // Configure EF Core InMemory database
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // EncryptionService dependency for DbContext
            var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Encryption:Key"] = base64Key })
                .Build();
            var encryption = new EncryptionService(config);

            // Initialize DbContext and service
            _db = new BiblioMateDbContext(options, encryption);
            _service = new TagService(_db);
        }

        public void Dispose() => _db.Dispose();

        // ----------------- Create -----------------

        /// <summary>
        /// CreateAsync should add a new tag and persist it in the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddTag()
        {
            var dto = new TagCreateDto { Name = "Classic" };

            var result = await _service.CreateAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal("Classic", result.Name);
            Assert.True(await _db.Tags.AnyAsync(t => t.Name == "Classic", _ct));
        }

        // ----------------- Read -----------------

        /// <summary>
        /// GetAllAsync should return all tags from the database.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTags()
        {
            _db.Tags.AddRange(new Tag { Name = "Adventure" }, new Tag { Name = "Coming-of-Age" });
            await _db.SaveChangesAsync(_ct);

            var tags = (await _service.GetAllAsync(_ct)).ToList();

            Assert.Equal(2, tags.Count);
            Assert.Contains(tags, t => t.Name == "Adventure");
            Assert.Contains(tags, t => t.Name == "Coming-of-Age");
        }

        /// <summary>
        /// GetByIdAsync should return the tag when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnTag_WhenExists()
        {
            var tag = _db.Tags.Add(new Tag { Name = "Post-apocalyptique" }).Entity;
            await _db.SaveChangesAsync(_ct);

            var dto = await _service.GetByIdAsync(tag.TagId, _ct);

            Assert.NotNull(dto);
            Assert.Equal(tag.TagId, dto!.TagId);
            Assert.Equal(tag.Name, dto.Name);
        }

        /// <summary>
        /// GetByIdAsync should return null when the tag does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var dto = await _service.GetByIdAsync(999, _ct);
            Assert.Null(dto);
        }

        // ----------------- Update -----------------

        /// <summary>
        /// UpdateAsync should modify an existing tag.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyTag_WhenExists()
        {
            var tag = _db.Tags.Add(new Tag { Name = "Old" }).Entity;
            await _db.SaveChangesAsync(_ct);

            var ok = await _service.UpdateAsync(new TagUpdateDto { TagId = tag.TagId, Name = "New" }, _ct);

            Assert.True(ok);
            var updated = await _db.Tags.FindAsync(new object[] { tag.TagId }, _ct);
            Assert.Equal("New", updated!.Name);
        }

        /// <summary>
        /// UpdateAsync should return false when the tag does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var ok = await _service.UpdateAsync(new TagUpdateDto { TagId = 12345, Name = "X" }, _ct);
            Assert.False(ok);
        }

        // ----------------- Delete -----------------

        /// <summary>
        /// DeleteAsync should remove a tag from the database when it exists.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveTag_WhenExists()
        {
            var tag = _db.Tags.Add(new Tag { Name = "ToDelete" }).Entity;
            await _db.SaveChangesAsync(_ct);

            var ok = await _service.DeleteAsync(tag.TagId, _ct);

            Assert.True(ok);
            Assert.False(await _db.Tags.AnyAsync(t => t.TagId == tag.TagId, _ct));
        }

        /// <summary>
        /// DeleteAsync should return false when the tag does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var ok = await _service.DeleteAsync(999, _ct);
            Assert.False(ok);
        }

        // ----------------- SearchAsync -----------------

        /// <summary>
        /// SearchAsync should return ordered results by name
        /// when search term is null, respecting the "take" limit.
        /// </summary>
        [Fact]
        public async Task SearchAsync_WhenSearchIsNull_TakesAndOrdersByName()
        {
            _db.Tags.AddRange(
                new Tag { Name = "zeta" },
                new Tag { Name = "alpha" },
                new Tag { Name = "beta" }
            );
            await _db.SaveChangesAsync(_ct);

            var res = (await _service.SearchAsync(null, take: 2, _ct)).ToList();

            Assert.Equal(2, res.Count);
            Assert.Equal(new[] { "alpha", "beta" }, res.Select(r => r.Name).ToArray());
        }

        /// <summary>
        /// SearchAsync should filter results case-insensitively
        /// when a search term is provided.
        /// </summary>
        [Fact]
        public async Task SearchAsync_FiltersBySubstring_CaseInsensitive()
        {
            _db.Tags.AddRange(
                new Tag { Name = "Fantasy" },
                new Tag { Name = "High Fantasy" },
                new Tag { Name = "Science Fiction" }
            );
            await _db.SaveChangesAsync(_ct);

            var res = (await _service.SearchAsync("fant", take: 10, _ct)).Select(r => r.Name).ToList();

            Assert.Equal(2, res.Count);
            Assert.Contains("Fantasy", res);
            Assert.Contains("High Fantasy", res);
        }

        /// <summary>
        /// SearchAsync should clamp the "take" parameter to a maximum of 100 results.
        /// </summary>
        [Fact]
        public async Task SearchAsync_ClampsTakeToMax100()
        {
            var tags = Enumerable.Range(1, 105).Select(i => new Tag { Name = $"Tag{i:D3}" });
            _db.Tags.AddRange(tags);
            await _db.SaveChangesAsync(_ct);

            var res = (await _service.SearchAsync(null, take: 500, _ct)).ToList();

            Assert.Equal(100, res.Count);
        }

        // ----------------- EnsureAsync -----------------

        /// <summary>
        /// EnsureAsync should return an existing tag if the name already exists.
        /// </summary>
        [Fact]
        public async Task EnsureAsync_ReturnsExisting_WhenAlreadyPresent()
        {
            var existing = _db.Tags.Add(new Tag { Name = "Existing" }).Entity;
            await _db.SaveChangesAsync(_ct);

            var (dto, created) = await _service.EnsureAsync("Existing", _ct);

            Assert.False(created);
            Assert.Equal(existing.TagId, dto.TagId);
            Assert.Equal("Existing", dto.Name);
        }

        /// <summary>
        /// EnsureAsync should create a new tag if the name does not exist.
        /// </summary>
        [Fact]
        public async Task EnsureAsync_Creates_WhenMissing()
        {
            var (dto, created) = await _service.EnsureAsync("BrandNew", _ct);

            Assert.True(created);
            Assert.True(dto.TagId > 0);
            Assert.Equal("BrandNew", dto.Name);
            Assert.True(await _db.Tags.AnyAsync(t => t.TagId == dto.TagId, _ct));
        }

        /// <summary>
        /// EnsureAsync should throw if the name is empty or invalid.
        /// </summary>
        [Fact]
        public async Task EnsureAsync_Throws_WhenNameEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.EnsureAsync("", _ct));
        }
    }
}
