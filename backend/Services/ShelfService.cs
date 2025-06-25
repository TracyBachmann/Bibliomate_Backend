using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IShelfService"/> using EF Core.
    /// </summary>
    public class ShelfService : IShelfService
    {
        private readonly BiblioMateDbContext _context;

        public ShelfService(BiblioMateDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShelfReadDto>> GetAllAsync(int? zoneId, int page, int pageSize)
        {
            var query = _context.Shelves
                                .Include(s => s.Zone)
                                .Include(s => s.Genre)
                                .AsQueryable();

            if (zoneId.HasValue)
                query = query.Where(s => s.ZoneId == zoneId.Value);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return items.Select(s => new ShelfReadDto
            {
                ShelfId     = s.ShelfId,
                Name        = s.Name,
                ZoneId      = s.ZoneId,
                ZoneName    = s.Zone.Name,
                GenreId     = s.GenreId,
                GenreName   = s.Genre.Name,
                Capacity    = s.Capacity,
                CurrentLoad = s.CurrentLoad
            });
        }

        public async Task<ShelfReadDto?> GetByIdAsync(int id)
        {
            var s = await _context.Shelves
                .Include(x => x.Zone)
                .Include(x => x.Genre)
                .FirstOrDefaultAsync(x => x.ShelfId == id);

            if (s == null) return null;

            return new ShelfReadDto
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

        public async Task<ShelfReadDto> CreateAsync(ShelfCreateDto dto)
        {
            var s = new Models.Shelf
            {
                Name        = dto.Name,
                ZoneId      = dto.ZoneId,
                GenreId     = dto.GenreId,
                Capacity    = dto.Capacity,
                CurrentLoad = 0
            };

            _context.Shelves.Add(s);
            await _context.SaveChangesAsync();

            // reload navigation props
            await _context.Entry(s).Reference(x => x.Zone).LoadAsync();
            await _context.Entry(s).Reference(x => x.Genre).LoadAsync();

            return new ShelfReadDto
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

        public async Task<bool> UpdateAsync(ShelfUpdateDto dto)
        {
            var s = await _context.Shelves.FindAsync(dto.ShelfId);
            if (s == null) return false;

            s.Name     = dto.Name;
            s.ZoneId   = dto.ZoneId;
            s.GenreId  = dto.GenreId;
            s.Capacity = dto.Capacity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var s = await _context.Shelves.FindAsync(id);
            if (s == null) return false;

            _context.Shelves.Remove(s);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}