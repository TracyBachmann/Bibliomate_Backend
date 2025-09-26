using System.Linq.Expressions;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Editor"/> entities
    /// using EF Core.
    /// </summary>
    public class EditorService : IEditorService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorService"/> class.
        /// </summary>
        /// <param name="db">Database context for BiblioMate.</param>
        public EditorService(BiblioMateDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Retrieves all editors from the data store.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{EditorReadDto}"/> containing all editors.
        /// </returns>
        public async Task<IEnumerable<EditorReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _db.Editors
                .AsNoTracking()
                .Select(MapToReadDto)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single editor by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the editor to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item>The <see cref="EditorReadDto"/> if found, otherwise null.</item>
        ///   <item>An <see cref="IActionResult"/> <c>NotFound</c> when missing, otherwise null.</item>
        /// </list>
        /// </returns>
        public async Task<(EditorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Editors.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return (null, new NotFoundResult());

            return (new EditorReadDto
            {
                EditorId = entity.EditorId,
                Name     = entity.Name
            }, null);
        }

        /// <summary>
        /// Creates a new editor in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing editor creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing the created <see cref="EditorReadDto"/> and a <see cref="CreatedAtActionResult"/>.
        /// </returns>
        public async Task<(EditorReadDto CreatedDto, CreatedAtActionResult Result)> CreateAsync(
            EditorCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Editor { Name = dto.Name };
            _db.Editors.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            var createdDto = new EditorReadDto
            {
                EditorId = entity.EditorId,
                Name     = entity.Name
            };

            var result = new CreatedAtActionResult(
                actionName: nameof(EditorsController.GetEditor),
                controllerName: "Editors",
                routeValues: new { id = createdDto.EditorId },
                value: createdDto);

            return (createdDto, result);
        }

        /// <summary>
        /// Updates an existing editor in the data store.
        /// </summary>
        /// <param name="id">Identifier of the editor to update.</param>
        /// <param name="dto">Data transfer object containing updated editor data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the editor was not found.
        /// </returns>
        public async Task<bool> UpdateAsync(
            int id,
            EditorUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Editors.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return false;

            entity.Name = dto.Name;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes an editor from the data store.
        /// </summary>
        /// <param name="id">Identifier of the editor to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; <c>false</c> if the editor was not found.
        /// </returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Editors.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return false;

            _db.Editors.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Expression to project <see cref="Editor"/> into <see cref="EditorReadDto"/>.
        /// </summary>
        private static readonly Expression<Func<Editor, EditorReadDto>> MapToReadDto = e => new EditorReadDto
        {
            EditorId = e.EditorId,
            Name     = e.Name
        };
        
        public async Task<IEnumerable<EditorReadDto>> SearchAsync(string? search, int take, CancellationToken ct)
        {
            var q = _db.Editors.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(e => e.Name.ToLower().Contains(s));
            }

            take = Math.Clamp(take, 1, 100);

            return await q.OrderBy(e => e.Name)
                .Take(take)
                .Select(MapToReadDto)
                .ToListAsync(ct);
        }

        public async Task<(EditorReadDto Dto, bool Created)> EnsureAsync(string name, CancellationToken ct)
        {
            var normalized = (name ?? "").Trim();
            if (normalized.Length < 2) throw new ArgumentException("Name too short", nameof(name));

            var existing = await _db.Editors
                .AsNoTracking()
                .Where(e => e.Name.ToLower() == normalized.ToLower())
                .Select(e => new EditorReadDto { EditorId = e.EditorId, Name = e.Name })
                .FirstOrDefaultAsync(ct);

            if (existing != null) return (existing, false);

            var entity = new Editor { Name = normalized };
            _db.Editors.Add(entity);
            await _db.SaveChangesAsync(ct);

            return (new EditorReadDto { EditorId = entity.EditorId, Name = entity.Name }, true);
        }

    }
}