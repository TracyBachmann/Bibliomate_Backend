using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Tag"/> entities using EF Core.
    /// </summary>
    public class TagService : ITagService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagService"/> class.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        public TagService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all tags from the data store.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TagReadDto}"/> containing all tags.
        /// </returns>
        public async Task<IEnumerable<TagReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
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
        public async Task<TagReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
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
        public async Task<TagReadDto> CreateAsync(
            TagCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Tag { Name = dto.Name };
            _context.Tags.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new TagReadDto
            {
                TagId = entity.TagId,
                Name  = entity.Name
            };
        }

        /// <summary>
        /// Updates an existing tag in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated tag data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the tag was not found.
        /// </returns>
        public async Task<bool> UpdateAsync(
            TagUpdateDto dto,
            CancellationToken cancellationToken = default)
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
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
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
    }
}