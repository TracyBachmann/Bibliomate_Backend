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
    public class TagsServiceTests
    {
        private readonly TagService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public TagsServiceTests(ITestOutputHelper output)
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

            _service = new TagService(_db);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddTag()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new TagCreateDto { Name = "Classic" };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created Tag: {result.Name}");

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Tags.AnyAsync(t => t.Name == dto.Name));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTags()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.Tags.Add(new Tag { Name = "Adventure" });
            _db.Tags.Add(new Tag { Name = "Coming-of-Age" });
            await _db.SaveChangesAsync();

            var tags = (await _service.GetAllAsync()).ToList();

            _output.WriteLine($"Found Tags Count: {tags.Count}");

            Assert.Equal(2, tags.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTag_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var tag = new Tag { Name = "Post-apocalyptique" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(tag.TagId);

            _output.WriteLine($"Found Tag: {dto?.Name}");

            Assert.NotNull(dto);
            Assert.Equal(tag.Name, dto.Name);

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
        public async Task UpdateAsync_ShouldModifyTag_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var tag = new Tag { Name = "Old Tag" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var dto = new TagUpdateDto { TagId = tag.TagId, Name = "Updated Tag" };
            var success = await _service.UpdateAsync(dto);

            _output.WriteLine($"Success: {success}");
            _output.WriteLine($"Updated Name: {(await _db.Tags.FindAsync(tag.TagId))?.Name}");

            Assert.True(success);
            Assert.Equal("Updated Tag", (await _db.Tags.FindAsync(tag.TagId))?.Name);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new TagUpdateDto { TagId = 999, Name = "Doesn't matter" };
            var success = await _service.UpdateAsync(dto);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTag_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var tag = new Tag { Name = "ToDelete" };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(tag.TagId);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.Tags.AnyAsync(t => t.TagId == tag.TagId));

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