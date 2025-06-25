using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines CRUD and query operations for shelves.
    /// </summary>
    public interface IShelfService
    {
        /// <summary>
        /// Retrieves a paged list of shelves, optionally filtered by zone.
        /// </summary>
        /// <param name="zoneId">Optional zone identifier to filter by.</param>
        /// <param name="page">Page index (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>A collection of <see cref="ShelfReadDto"/>.</returns>
        Task<IEnumerable<ShelfReadDto>> GetAllAsync(int? zoneId, int page, int pageSize);

        /// <summary>
        /// Retrieves a single shelf by its identifier.
        /// </summary>
        /// <param name="id">Shelf identifier.</param>
        /// <returns>The <see cref="ShelfReadDto"/> if found; otherwise null.</returns>
        Task<ShelfReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new shelf.
        /// </summary>
        /// <param name="dto">The data required to create the shelf.</param>
        /// <returns>The created <see cref="ShelfReadDto"/>.</returns>
        Task<ShelfReadDto> CreateAsync(ShelfCreateDto dto);

        /// <summary>
        /// Updates an existing shelf.
        /// </summary>
        /// <param name="dto">The updated shelf data.</param>
        /// <returns>True if update succeeded; false if shelf not found.</returns>
        Task<bool> UpdateAsync(ShelfUpdateDto dto);

        /// <summary>
        /// Deletes a shelf by its identifier.
        /// </summary>
        /// <param name="id">Shelf identifier.</param>
        /// <returns>True if deletion succeeded; false if shelf not found.</returns>
        Task<bool> DeleteAsync(int id);
    }
}