using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides hierarchical access to library locations (floors, aisles, shelves, levels).
    /// Also supports "ensure" semantics: creates missing Zone/Shelf/Level entries on demand.
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        public LocationService(BiblioMateDbContext db) => _db = db;

        /// <inheritdoc/>
        public async Task<IEnumerable<FloorReadDto>> GetFloorsAsync(CancellationToken ct = default)
        {
            return await _db.Zones.AsNoTracking()
                .Select(z => z.FloorNumber)
                .Distinct()
                .OrderBy(n => n)
                .Select(n => new FloorReadDto(n))
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AisleReadDto>> GetAislesAsync(int floorNumber, CancellationToken ct = default)
        {
            return await _db.Zones.AsNoTracking()
                .Where(z => z.FloorNumber == floorNumber)
                .Select(z => z.AisleCode)
                .Distinct()
                .OrderBy(a => a)
                .Select(a => new AisleReadDto(a))
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ShelfMiniReadDto>> GetShelvesAsync(int floorNumber, string aisleCode, CancellationToken ct = default)
        {
            return await _db.Shelves.AsNoTracking()
                .Include(s => s.Zone)
                .Where(s => s.Zone.FloorNumber == floorNumber && s.Zone.AisleCode == aisleCode)
                .OrderBy(s => s.Name)
                .Select(s => new ShelfMiniReadDto(s.ShelfId, s.Name))
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LevelReadDto>> GetLevelsAsync(int shelfId, CancellationToken ct = default)
        {
            return await _db.ShelfLevels.AsNoTracking()
                .Where(l => l.ShelfId == shelfId)
                .OrderBy(l => l.LevelNumber)
                .Select(l => new LevelReadDto(l.LevelNumber))
                .ToListAsync(ct);
        }

        /// <summary>
        /// Ensures that a Zone, Shelf, and ShelfLevel exist for the given parameters.
        /// Creates missing entries as needed and returns the fully resolved location.
        /// </summary>
        /// <param name="dto">
        /// The input data specifying floor number, aisle code, shelf name, and level number.
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// A <see cref="LocationReadDto"/> containing identifiers and descriptive info
        /// for the ensured Zone, Shelf, and ShelfLevel.
        /// </returns>
        public async Task<LocationReadDto> EnsureAsync(LocationEnsureDto dto, CancellationToken ct = default)
        {
            // 1) Zone (Floor + Aisle)
            var zone = await _db.Zones
                .FirstOrDefaultAsync(z => z.FloorNumber == dto.FloorNumber && z.AisleCode == dto.AisleCode, ct);

            if (zone is null)
            {
                zone = new Zone
                {
                    Name = $"{dto.FloorNumber}-{dto.AisleCode}",
                    FloorNumber = dto.FloorNumber,
                    AisleCode = dto.AisleCode
                };
                _db.Zones.Add(zone);
                await _db.SaveChangesAsync(ct);
            }

            // 2) Shelf (Rayon)
            var shelf = await _db.Shelves
                .FirstOrDefaultAsync(s => s.ZoneId == zone.ZoneId && s.Name == dto.ShelfName, ct);

            if (shelf is null)
            {
                shelf = new Shelf
                {
                    ZoneId = zone.ZoneId,
                    Name = dto.ShelfName,
                    GenreId = 1,          // default "neutral" genre; can be adjusted
                    Capacity = 0,
                    CurrentLoad = 0
                };
                _db.Shelves.Add(shelf);
                await _db.SaveChangesAsync(ct);
            }

            // 3) ShelfLevel (Étagère)
            var level = await _db.ShelfLevels
                .FirstOrDefaultAsync(l => l.ShelfId == shelf.ShelfId && l.LevelNumber == dto.LevelNumber, ct);

            if (level is null)
            {
                level = new ShelfLevel
                {
                    ShelfId = shelf.ShelfId,
                    LevelNumber = dto.LevelNumber,
                    Capacity = 0,
                    CurrentLoad = 0,
                    MaxHeight = 0
                };
                _db.ShelfLevels.Add(level);
                await _db.SaveChangesAsync(ct);
            }

            return new LocationReadDto
            {
                ZoneId = zone.ZoneId,
                ShelfId = shelf.ShelfId,
                ShelfLevelId = level.ShelfLevelId,
                FloorNumber = zone.FloorNumber,
                AisleCode = zone.AisleCode,
                ShelfName = shelf.Name,
                LevelNumber = level.LevelNumber
            };
        }
    }
}

