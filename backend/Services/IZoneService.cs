using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines operations for managing library zones.
    /// </summary>
    public interface IZoneService
    {
        /// <summary>
        /// Retrieves a paginated list of zones.
        /// </summary>
        /// <param name="page">1-based page index.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>A collection of <see cref="ZoneReadDto"/>.</returns>
        Task<IEnumerable<ZoneReadDto>> GetAllAsync(int page, int pageSize);

        /// <summary>
        /// Retrieves a zone by its identifier.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <returns>
        /// The <see cref="ZoneReadDto"/> if found; otherwise null.
        /// </returns>
        Task<ZoneReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new zone.
        /// </summary>
        /// <param name="dto">The data to create the zone.</param>
        /// <returns>The created <see cref="ZoneReadDto"/>.</returns>
        Task<ZoneReadDto> CreateAsync(ZoneCreateDto dto);

        /// <summary>
        /// Updates an existing zone.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <param name="dto">The updated data.</param>
        /// <returns>
        /// True if update succeeded; false if zone not found.
        /// </returns>
        Task<bool> UpdateAsync(int id, ZoneUpdateDto dto);

        /// <summary>
        /// Deletes a zone.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <returns>
        /// True if deletion succeeded; false if zone not found.
        /// </returns>
        Task<bool> DeleteAsync(int id);
    }
}