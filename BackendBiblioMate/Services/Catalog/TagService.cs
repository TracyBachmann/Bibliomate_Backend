using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD and query operations for <see cref="Tag"/> entities using EF Core.
    /// </summary>
    public class TagService : ITagService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagService"/> class.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        public TagService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TagReadDto}"/> containing all tags.
        /// </returns>
        public async Task<IEnumerable<TagReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .AsNoTracking()
                .Select(MapToDto)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single tag by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the tag to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <see cref="TagReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<TagReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .AsNoTracking()
                .Where(t => t.TagId == id)
                .Select(MapToDto)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a new tag in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing tag creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The created <see cref="TagReadDto"/>.</returns>
        public async Task<TagReadDto> CreateAsync(TagCreateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new Tag { Name = dto.Name };
            _context.Tags.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new TagReadDto { TagId = entity.TagId, Name = entity.Name };
        }

        /// <summary>
        /// Updates an existing tag in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated tag data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the tag was not found.
        /// </returns>
        public async Task<bool> UpdateAsync(TagUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Tags
                .FirstOrDefaultAsync(t => t.TagId == dto.TagId, cancellationToken);

            if (entity is null)
                return false;

            entity.Name = dto.Name;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes a tag from the data store.
        /// </summary>
        /// <param name="id">Identifier of the tag to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; <c>false</c> if the tag was not found.
        /// </returns>
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
                return false;

            _context.Tags.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Expression to project <see cref="Tag"/> into <see cref="TagReadDto"/>.
        /// </summary>
        private static readonly Expression<Func<Tag, TagReadDto>> MapToDto = t => new TagReadDto
        {
            TagId = t.TagId,
            Name  = t.Name
        };

        // ===== Extra features: Search + Ensure ==============================

        /// <summary>
        /// Searches for tags by name, returning up to <paramref name="take"/> results.
        /// </summary>
        /// <param name="search">Optional search term; if <c>null</c> or empty, returns all.</param>
        /// <param name="take">Maximum number of results to return (1–100).</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A list of matching <see cref="TagReadDto"/> results.
        /// </returns>
        public async Task<IEnumerable<TagReadDto>> SearchAsync(string? search, int take, CancellationToken ct)
        {
            var q = _context.Tags.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(t => t.Name.ToLower().Contains(s));
            }

            take = Math.Clamp(take, 1, 100);

            return await q.OrderBy(t => t.Name)
                          .Take(take)
                          .Select(MapToDto)
                          .ToListAsync(ct);
        }

        /// <summary>
        /// Ensures that a tag with the given name exists.
        /// Creates it if missing and returns whether it was newly created.
        /// </summary>
        /// <param name="name">The tag name to ensure.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing the <see cref="TagReadDto"/> and a boolean indicating if it was newly created.
        /// </returns>
        public async Task<(TagReadDto Dto, bool Created)> EnsureAsync(string name, CancellationToken ct)
        {
            var normalized = (name ?? "").Trim();
            if (normalized.Length < 1) throw new ArgumentException("Name too short", nameof(name));

            var existing = await _context.Tags
                .AsNoTracking()
                .Where(t => t.Name.ToLower() == normalized.ToLower())
                .Select(MapToDto)
                .FirstOrDefaultAsync(ct);

            if (existing != null) return (existing, false);

            var entity = new Tag { Name = normalized };
            _context.Tags.Add(entity);
            await _context.SaveChangesAsync(ct);

            return (new TagReadDto { TagId = entity.TagId, Name = entity.Name }, true);
        }
    }
}

