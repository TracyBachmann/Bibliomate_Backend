using backend.Models;
using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IZoneService"/> using EF Core.
    /// </summary>
    public class ZoneService : IZoneService
    {
        private readonly BiblioMateDbContext _context;

        public ZoneService(BiblioMateDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ZoneReadDto>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Zones
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(z => new ZoneReadDto
                {
                    ZoneId      = z.ZoneId,
                    FloorNumber = z.FloorNumber,
                    AisleCode   = z.AisleCode,
                    Description = z.Description
                })
                .ToListAsync();
        }

        public async Task<ZoneReadDto?> GetByIdAsync(int id)
        {
            var z = await _context.Zones.FindAsync(id);
            if (z == null) return null;
            return new ZoneReadDto
            {
                ZoneId      = z.ZoneId,
                FloorNumber = z.FloorNumber,
                AisleCode   = z.AisleCode,
                Description = z.Description
            };
        }

        public async Task<ZoneReadDto> CreateAsync(ZoneCreateDto dto)
        {
            var z = new Zone
            {
                FloorNumber = dto.FloorNumber,
                AisleCode   = dto.AisleCode,
                Description = dto.Description
            };
            _context.Zones.Add(z);
            await _context.SaveChangesAsync();
            return new ZoneReadDto
            {
                ZoneId      = z.ZoneId,
                FloorNumber = z.FloorNumber,
                AisleCode   = z.AisleCode,
                Description = z.Description
            };
        }

        public async Task<bool> UpdateAsync(int id, ZoneUpdateDto dto)
        {
            var z = await _context.Zones.FindAsync(id);
            if (z == null) return false;
            z.FloorNumber = dto.FloorNumber;
            z.AisleCode   = dto.AisleCode;
            z.Description = dto.Description;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var z = await _context.Zones.FindAsync(id);
            if (z == null) return false;
            _context.Zones.Remove(z);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
