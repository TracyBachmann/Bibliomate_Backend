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
    public class EditorsServiceTests
    {
        private readonly EditorService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public EditorsServiceTests(ITestOutputHelper output)
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

            _service = new EditorService(_db);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEditor()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            var dto = new EditorCreateDto { Name = "Penguin Random House" };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created Editor: {result.Name}");

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.True(await _db.Editors.AnyAsync(e => e.Name == dto.Name));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEditors()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.Editors.Add(new Editor { Name = "Editor 1" });
            _db.Editors.Add(new Editor { Name = "Editor 2" });
            await _db.SaveChangesAsync();

            var editors = (await _service.GetAllAsync()).ToList();
            
            _output.WriteLine($"Found Editors Count: {editors.Count()}");

            Assert.Equal(2, editors.Count());

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEditor_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var editor = new Editor { Name = "Specific Editor" };
            _db.Editors.Add(editor);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(editor.EditorId);

            _output.WriteLine($"Found Editor: {dto?.Name}");

            Assert.NotNull(dto);
            Assert.Equal(editor.Name, dto.Name);

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
        public async Task UpdateAsync_ShouldModifyEditor_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var editor = new Editor { Name = "Old Name" };
            _db.Editors.Add(editor);
            await _db.SaveChangesAsync();

            var dto = new EditorCreateDto { Name = "New Name" };
            var success = await _service.UpdateAsync(editor.EditorId, dto);

            _output.WriteLine($"Success: {success}");
            _output.WriteLine($"Updated Name: {(await _db.Editors.FindAsync(editor.EditorId))?.Name}");

            Assert.True(success);
            Assert.Equal("New Name", (await _db.Editors.FindAsync(editor.EditorId))?.Name);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new EditorCreateDto { Name = "Doesn't matter" };
            var success = await _service.UpdateAsync(999, dto);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEditor_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var editor = new Editor { Name = "ToDelete" };
            _db.Editors.Add(editor);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(editor.EditorId);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.Editors.AnyAsync(e => e.EditorId == editor.EditorId));

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