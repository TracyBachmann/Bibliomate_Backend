using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Shelf"/> entities using EF Core.
    /// </summary>
    public class ShelfService : IShelfService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShelfService"/> class.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        public ShelfService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all shelves, optionally filtered by zone ID, with pagination.
        /// </summary>
        /// <param name="zoneId">Optional zone ID to filter by.</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{ShelfReadDto}"/> containing the paginated results.
        /// </returns>
        public async Task<IEnumerable<ShelfReadDto>> GetAllAsync(
            int? zoneId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Shelves
                .AsNoTracking()
                .Include(s => s.Zone)
                .Include(s => s.Genre)
                .AsQueryable();

            if (zoneId.HasValue)
                query = query.Where(s => s.ZoneId == zoneId.Value);

            return await query
                .OrderBy(s => s.ShelfId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single shelf by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shelf to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <see cref="ShelfReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<ShelfReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Shelves
                .AsNoTracking()
                .Include(s => s.Zone)
                .Include(s => s.Genre)
                .Where(s => s.ShelfId == id)
                .Select(MapToDto)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a new shelf in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing shelf creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The created <see cref="ShelfReadDto"/>.</returns>
        public async Task<ShelfReadDto> CreateAsync(
            ShelfCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Shelf
            {
                Name        = dto.Name,
                ZoneId      = dto.ZoneId,
                GenreId     = dto.GenreId,
                Capacity    = dto.Capacity,
                CurrentLoad = 0
            };

            _context.Shelves.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(entity).Reference(e => e.Zone).LoadAsync(cancellationToken);
            await _context.Entry(entity).Reference(e => e.Genre).LoadAsync(cancellationToken);

            return new ShelfReadDto
            {
                ShelfId     = entity.ShelfId,
                Name        = entity.Name,
                ZoneId      = entity.ZoneId,
                ZoneName    = entity.Zone.Name,
                GenreId     = entity.GenreId,
                GenreName   = entity.Genre.Name,
                Capacity    = entity.Capacity,
                CurrentLoad = entity.CurrentLoad
            };
        }

        /// <summary>
        /// Updates an existing shelf in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated shelf data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the shelf was not found.
        /// </returns>
        public async Task<bool> UpdateAsync(
            ShelfUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Shelves.FindAsync([dto.ShelfId], cancellationToken);
            if (entity is null)
                return false;

            entity.Name     = dto.Name;
            entity.ZoneId   = dto.ZoneId;
            entity.GenreId  = dto.GenreId;
            entity.Capacity = dto.Capacity;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes a shelf from the data store.
        /// </summary>
        /// <param name="id">Identifier of the shelf to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; <c>false</c> if the shelf was not found.
        /// </returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Shelves.FindAsync([id], cancellationToken);
            if (entity is null)
                return false;

            _context.Shelves.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Expression to project <see cref="Shelf"/> into <see cref="ShelfReadDto"/>.
        /// </summary>
        private static readonly Expression<Func<Shelf, ShelfReadDto>> MapToDto = s => new ShelfReadDto
        {
            ShelfId     = s.ShelfId,
            Name        = s.Name,
            ZoneId      = s.ZoneId,
            ZoneName    = s.Zone.Name,
            GenreId     = s.GenreId,
            GenreName   = s.Genre.Name,
            Capacity    = s.Capacity,
            CurrentLoad = s.CurrentLoad
        };
    }
}
