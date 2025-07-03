using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for ShelfLevel entities using EF Core.
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

        /// <summary>
        /// Retrieves all shelf levels, optionally filtered by shelf ID, with pagination.
        /// </summary>
        /// <param name="shelfId">Optional shelf ID to filter by.</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{ShelfLevelReadDto}"/> containing the paginated results.
        /// </returns>
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

        /// <summary>
        /// Retrieves a single shelf level by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shelf level to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <see cref="ShelfLevelReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
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

        /// <summary>
        /// Creates a new shelf level in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing shelf level creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The created <see cref="ShelfLevelReadDto"/>.</returns>
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

        /// <summary>
        /// Updates an existing shelf level in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated shelf level data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the shelf level was not found.
        /// </returns>
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

        /// <summary>
        /// Deletes a shelf level from the data store.
        /// </summary>
        /// <param name="id">Identifier of the shelf level to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; <c>false</c> if the shelf level was not found.
        /// </returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var sl = await _context.ShelfLevels.FindAsync(new object[] { id }, cancellationToken);
            if (sl == null)
                return false;

            _context.ShelfLevels.Remove(sl);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Expression to project <see cref="Models.ShelfLevel"/> into <see cref="ShelfLevelReadDto"/>.
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