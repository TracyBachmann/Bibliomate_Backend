using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Models.ShelfLevel"/> entities using EF Core.
    /// </summary>
    public class ShelfLevelService : IShelfLevelService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShelfLevelService"/> class.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        public ShelfLevelService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ShelfLevelReadDto>> GetAllAsync(
            int? shelfId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ShelfLevels
                .AsNoTracking()
                .Include(sl => sl.Shelf)
                .AsQueryable();

            if (shelfId.HasValue)
                query = query.Where(sl => sl.ShelfId == shelfId.Value);

            return await query
                .OrderBy(sl => sl.ShelfLevelId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToReadDto)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ShelfLevelReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.ShelfLevels
                .AsNoTracking()
                .Include(x => x.Shelf)
                .Where(x => x.ShelfLevelId == id)
                .Select(MapToReadDto)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ShelfLevelReadDto> CreateAsync(
            ShelfLevelCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var sl = new Models.ShelfLevel
            {
                LevelNumber = dto.LevelNumber,
                ShelfId     = dto.ShelfId,
                MaxHeight   = dto.MaxHeight ?? 0,
                Capacity    = dto.Capacity  ?? 0,
                CurrentLoad = dto.CurrentLoad ?? 0
            };

            _context.ShelfLevels.Add(sl);
            await _context.SaveChangesAsync(cancellationToken);

            var shelf = await _context.Shelves
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ShelfId == sl.ShelfId, cancellationToken);

            return new ShelfLevelReadDto
            {
                ShelfLevelId = sl.ShelfLevelId,
                LevelNumber  = sl.LevelNumber,
                ShelfId      = sl.ShelfId,
                ShelfName    = shelf?.Name ?? string.Empty,
                MaxHeight    = sl.MaxHeight,
                Capacity     = sl.Capacity,
                CurrentLoad  = sl.CurrentLoad
            };
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(
            ShelfLevelUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var sl = await _context.ShelfLevels
                .FirstOrDefaultAsync(x => x.ShelfLevelId == dto.ShelfLevelId, cancellationToken);

            if (sl == null)
                return false;

            sl.LevelNumber  = dto.LevelNumber;
            sl.ShelfId      = dto.ShelfId;
            sl.MaxHeight    = dto.MaxHeight ?? 0;
            sl.Capacity     = dto.Capacity  ?? 0;
            sl.CurrentLoad  = dto.CurrentLoad ?? 0;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var sl = await _context.ShelfLevels.FindAsync([id], cancellationToken);
            if (sl == null)
                return false;

            _context.ShelfLevels.Remove(sl);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Projection expression mapping <see cref="Models.ShelfLevel"/> to <see cref="ShelfLevelReadDto"/>.
        /// Used to keep EF queries translatable.
        /// </summary>
        private static readonly Expression<Func<Models.ShelfLevel, ShelfLevelReadDto>> MapToReadDto = sl => new ShelfLevelReadDto
        {
            ShelfLevelId = sl.ShelfLevelId,
            LevelNumber  = sl.LevelNumber,
            ShelfId      = sl.ShelfId,
            ShelfName    = sl.Shelf.Name,
            MaxHeight    = sl.MaxHeight,
            Capacity     = sl.Capacity,
            CurrentLoad  = sl.CurrentLoad
        };
    }
}
