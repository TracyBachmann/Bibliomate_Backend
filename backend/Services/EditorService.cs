using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IEditorService"/> by coordinating EF Core
    /// to perform CRUD on <see cref="Editor"/> entities.
    /// </summary>
    public class EditorService : IEditorService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes a new instance of <see cref="EditorService"/>.
        /// </summary>
        /// <param name="db">The EF Core database context.</param>
        public EditorService(BiblioMateDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EditorReadDto>> GetAllAsync()
        {
            return await _db.Editors
                .Select(e => new EditorReadDto
                {
                    EditorId = e.EditorId,
                    Name     = e.Name
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<EditorReadDto?> GetByIdAsync(int id)
        {
            var e = await _db.Editors.FindAsync(id);
            if (e == null) return null;

            return new EditorReadDto
            {
                EditorId = e.EditorId,
                Name     = e.Name
            };
        }

        /// <inheritdoc/>
        public async Task<EditorReadDto> CreateAsync(EditorCreateDto dto)
        {
            var entity = new Editor { Name = dto.Name };
            _db.Editors.Add(entity);
            await _db.SaveChangesAsync();

            return new EditorReadDto
            {
                EditorId = entity.EditorId,
                Name     = entity.Name
            };
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(int id, EditorCreateDto dto)
        {
            var entity = await _db.Editors.FindAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name;
            await _db.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Editors.FindAsync(id);
            if (entity == null) return false;

            _db.Editors.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}