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
    /// Unit tests for <see cref="EditorService"/>.
    /// Verifies CRUD operations using an in-memory EF Core provider.
    /// </summary>
    public class EditorServiceTest
    {
        private readonly EditorService _service;
        private readonly BiblioMateDbContext _db;
        private readonly CancellationToken _ct = CancellationToken.None;

        /// <summary>
        /// Initializes the test context with in-memory EF Core and encryption.
        /// </summary>
        public EditorServiceTest()
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
            _service = new EditorService(_db);
        }

        /// <summary>
        /// Verifies that CreateAsync adds a new editor to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddEditor()
        {
            // Arrange
            var dto = new EditorCreateDto { Name = "Penguin Random House" };

            // Act
            var (createdDto, _) = await _service.CreateAsync(dto, _ct);

            // Assert
            Assert.NotNull(createdDto);
            Assert.Equal(dto.Name, createdDto.Name);
            Assert.True(await _db.Editors.AnyAsync(e => e.Name == dto.Name, _ct));
        }

        /// <summary>
        /// Verifies that GetAllAsync returns all existing editors.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEditors()
        {
            // Arrange
            _db.Editors.AddRange(
                new Editor { Name = "Editor 1" },
                new Editor { Name = "Editor 2" }
            );
            await _db.SaveChangesAsync(_ct);

            // Act
            var editors = (await _service.GetAllAsync(_ct)).ToList();

            // Assert
            Assert.Equal(2, editors.Count);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the correct editor when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEditor_WhenExists()
        {
            // Arrange
            var editor = new Editor { Name = "Specific Editor" };
            _db.Editors.Add(editor);
            await _db.SaveChangesAsync(_ct);

            // Act
            var (dto, error) = await _service.GetByIdAsync(editor.EditorId, _ct);

            // Assert
            Assert.Null(error);
            Assert.NotNull(dto);
            Assert.Equal(editor.Name, dto.Name);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns a NotFoundResult when the editor does not exist.
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
        /// Verifies that UpdateAsync successfully updates an existing editor.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyEditor_WhenExists()
        {
            // Arrange
            var editor = new Editor { Name = "Old Name" };
            _db.Editors.Add(editor);
            await _db.SaveChangesAsync(_ct);

            var dto = new EditorUpdateDto { Name = "New Name" };

            // Act
            var success = await _service.UpdateAsync(editor.EditorId, dto, _ct);

            // Assert
            Assert.True(success);
            var updated = await _db.Editors.FindAsync(
                new object[] { editor.EditorId }, _ct);
            Assert.Equal("New Name", updated?.Name);
        }

        /// <summary>
        /// Verifies that UpdateAsync returns false when the editor to update does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var dto = new EditorUpdateDto { Name = "Doesn't matter" };

            // Act
            var success = await _service.UpdateAsync(999, dto, _ct);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        /// Verifies that DeleteAsync removes an existing editor.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveEditor_WhenExists()
        {
            // Arrange
            var editor = new Editor { Name = "ToDelete" };
            _db.Editors.Add(editor);
            await _db.SaveChangesAsync(_ct);

            // Act
            var success = await _service.DeleteAsync(editor.EditorId, _ct);

            // Assert
            Assert.True(success);
            Assert.False(await _db.Editors
                .AnyAsync(e => e.EditorId == editor.EditorId, _ct));
        }

        /// <summary>
        /// Verifies that DeleteAsync returns false when the editor to delete does not exist.
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