using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Zone"/> entities using EF Core.
    /// </summary>
    public class ZoneService : IZoneService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneService"/> class.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        public ZoneService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all zones with pagination.
        /// </summary>
        /// <param name="page">Page index (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{ZoneReadDto}"/> containing the requested page of zones.
        /// </returns>
        public async Task<IEnumerable<ZoneReadDto>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.Zones
                .AsNoTracking()
                .OrderBy(z => z.ZoneId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single zone by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the zone to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <see cref="ZoneReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<ZoneReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Zones
                .AsNoTracking()
                .Where(z => z.ZoneId == id)
                .Select(MapToDto)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a new zone in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing zone creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The created <see cref="ZoneReadDto"/>.
        /// </returns>
        public async Task<ZoneReadDto> CreateAsync(
            ZoneCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Zone
            {
                Name        = dto.Name,
                FloorNumber = dto.FloorNumber,
                AisleCode   = dto.AisleCode,
                Description = dto.Description
            };

            _context.Zones.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new ZoneReadDto
            {
                ZoneId      = entity.ZoneId,
                Name        = entity.Name,
                FloorNumber = entity.FloorNumber,
                AisleCode   = entity.AisleCode,
                Description = entity.Description
            };
        }

        /// <summary>
        /// Updates an existing zone in the data store.
        /// </summary>
        /// <param name="id">Identifier of the zone to update.</param>
        /// <param name="dto">Data transfer object containing updated zone data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; otherwise <c>false</c> if the zone was not found.
        /// </returns>
        public async Task<bool> UpdateAsync(
            int id,
            ZoneUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Zones.FindAsync(new object[] { id }, cancellationToken);

            if (entity is null)
                return false;

            entity.Name        = dto.Name;
            entity.FloorNumber = dto.FloorNumber;
            entity.AisleCode   = dto.AisleCode;
            entity.Description = dto.Description;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes a zone from the data store.
        /// </summary>
        /// <param name="id">Identifier of the zone to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; otherwise <c>false</c> if the zone was not found.
        /// </returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Zones.FindAsync(new object[] { id }, cancellationToken);

            if (entity is null)
                return false;

            _context.Zones.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Expression to project <see cref="Zone"/> into <see cref="ZoneReadDto"/>.
        /// </summary>
        private static readonly Expression<Func<Zone, ZoneReadDto>> MapToDto = z => new ZoneReadDto
        {
            ZoneId      = z.ZoneId,
            Name        = z.Name,
            FloorNumber = z.FloorNumber,
            AisleCode   = z.AisleCode,
            Description = z.Description
        };
    }
}
