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
    public class AuthorsServiceTest
    {
        private readonly AuthorService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public AuthorsServiceTest(ITestOutputHelper output)
        {
            _output = output;

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Exactement la même config que pour AuthServiceTests
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);
            _service = new AuthorService(_db);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddAuthor()
        {
            _output.WriteLine("=== CreateAsync: START ===");
            var dto = new AuthorCreateDto { Name = "Test Author" };

            var (result, actionResult) = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Authors.AnyAsync(a => a.Name == dto.Name));
            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllAuthors()
        {
            _output.WriteLine("=== GetAllAsync: START ===");
            _db.Authors.Add(new Author { Name = "Author 1" });
            _db.Authors.Add(new Author { Name = "Author 2" });
            await _db.SaveChangesAsync();

            var authors = await _service.GetAllAsync();

            Assert.Equal(2, authors.Count());
            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnAuthor_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (Exists): START ===");
            var author = new Author { Name = "Specific Author" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            var (result, error) = await _service.GetByIdAsync(author.AuthorId);

            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal(author.Name, result.Name);
            _output.WriteLine("=== GetByIdAsync (Exists): END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (NotExists): START ===");
            var (result, error) = await _service.GetByIdAsync(999);

            Assert.Null(result);
            Assert.NotNull(error);
            _output.WriteLine("=== GetByIdAsync (NotExists): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyAuthor_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (Exists): START ===");
            var author = new Author { Name = "Old Name" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            var dto = new AuthorCreateDto { Name = "New Name" };
            var success = await _service.UpdateAsync(author.AuthorId, dto);

            Assert.True(success);
            Assert.Equal("New Name", (await _db.Authors.FindAsync(author.AuthorId))?.Name);
            _output.WriteLine("=== UpdateAsync (Exists): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (NotExists): START ===");
            var dto = new AuthorCreateDto { Name = "Doesn't matter" };
            var success = await _service.UpdateAsync(999, dto);

            Assert.False(success);
            _output.WriteLine("=== UpdateAsync (NotExists): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveAuthor_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (Exists): START ===");
            var author = new Author { Name = "ToDelete" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(author.AuthorId);

            Assert.True(success);
            Assert.False(await _db.Authors.AnyAsync(a => a.AuthorId == author.AuthorId));
            _output.WriteLine("=== DeleteAsync (Exists): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (NotExists): START ===");
            var success = await _service.DeleteAsync(999);

            Assert.False(success);
            _output.WriteLine("=== DeleteAsync (NotExists): END ===");
        }
    }
}