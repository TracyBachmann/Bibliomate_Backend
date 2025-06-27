using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IShelfLevelService"/> using EF Core.
    /// </summary>
    public class ShelfLevelService : IShelfLevelService
    {
        private readonly BiblioMateDbContext _context;

        public ShelfLevelService(BiblioMateDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShelfLevelReadDto>> GetAllAsync(int? shelfId, int page, int pageSize)
        {
            var query = _context.ShelfLevels
                                .Include(sl => sl.Shelf)
                                .AsQueryable();

            if (shelfId.HasValue)
                query = query.Where(sl => sl.ShelfId == shelfId.Value);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return items.Select(sl => new ShelfLevelReadDto
            {
                ShelfLevelId = sl.ShelfLevelId,
                LevelNumber  = sl.LevelNumber,
                ShelfId      = sl.ShelfId,
                ShelfName    = sl.Shelf.Name,
                MaxHeight    = sl.MaxHeight,
                Capacity     = sl.Capacity,
                CurrentLoad  = sl.CurrentLoad
            });
        }

        public async Task<ShelfLevelReadDto?> GetByIdAsync(int id)
        {
            var sl = await _context.ShelfLevels
                .Include(x => x.Shelf)
                .FirstOrDefaultAsync(x => x.ShelfLevelId == id);

            if (sl == null) return null;

            return new ShelfLevelReadDto
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

        public async Task<ShelfLevelReadDto> CreateAsync(ShelfLevelCreateDto dto)
        {
            var sl = new Models.ShelfLevel
            {
                LevelNumber = dto.LevelNumber,
                ShelfId     = dto.ShelfId,
                MaxHeight   = dto.MaxHeight,
                Capacity    = dto.Capacity,
                CurrentLoad = dto.CurrentLoad
            };

            _context.ShelfLevels.Add(sl);
            await _context.SaveChangesAsync();

            // Reload shelf name
            var shelf = await _context.Shelves.FindAsync(sl.ShelfId);

            return new ShelfLevelReadDto
            {
                ShelfLevelId = sl.ShelfLevelId,
                LevelNumber  = sl.LevelNumber,
                ShelfId      = sl.ShelfId,
                ShelfName    = shelf?.Name ?? "Unknown",
                MaxHeight    = sl.MaxHeight,
                Capacity     = sl.Capacity,
                CurrentLoad  = sl.CurrentLoad
            };
        }

        public async Task<bool> UpdateAsync(ShelfLevelUpdateDto dto)
        {
            var sl = await _context.ShelfLevels.FindAsync(dto.ShelfLevelId);
            if (sl == null) return false;

            sl.LevelNumber = dto.LevelNumber;
            sl.ShelfId     = dto.ShelfId;
            sl.MaxHeight   = dto.MaxHeight;
            sl.Capacity    = dto.Capacity;
            sl.CurrentLoad = dto.CurrentLoad;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sl = await _context.ShelfLevels.FindAsync(id);
            if (sl == null) return false;

            _context.ShelfLevels.Remove(sl);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
